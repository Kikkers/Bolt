using Helpers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BodyController))]
public class TestCharacterController : MonoBehaviour
{
	[InlineEditor] [SerializeField] private PlayerConfig playerConfig;
	[SerializeField] private Animator animator;
	[SerializeField] private Transform camNodeRoot;

	private readonly List<Transform> camNodes = new List<Transform>();
	private readonly List<Collider> colliders = new List<Collider>();
	private InputMaster inputs;

	private Vector2 directionInput;
	private bool jumpInput;
	private bool crouchInput;
	private bool altInput;
	private Vector2 aimDeltaInput;

	private float currentJumpTime;

	private Vector3 worldLookDir;
	private Quaternion moveRotation;
	private Quaternion moveRotationDamp = Quaternion.identity;

	public Transform LookRoot => camNodeRoot;
	public IReadOnlyList<Transform> CamNodes => camNodes;
	public BodyController BodyController { get; private set; }

	public delegate void MotionUpdate(float deltaTime);
	public event MotionUpdate MotionUpdated;

	private void Awake()
	{
		BodyController = GetComponent<BodyController>();
		BodyController.GetComponentsInChildren(colliders);
		camNodeRoot.GetComponentsInChildren(camNodes);

		inputs = new InputMaster();
		inputs.Character.Jump.performed += JumpPerformed;
		inputs.Character.Crouch.performed += CrouchPerformed;
		inputs.Character.AltMode.performed += AltPerformed;

		inputs.Character.Direction.started += DirectionChanged;
		inputs.Character.Direction.performed += DirectionChanged;
		inputs.Character.Direction.canceled += DirectionChanged;

		inputs.Character.Aim.started += AimChanged;
		inputs.Character.Aim.performed += AimChanged;
		inputs.Character.Aim.canceled += AimChanged;

		inputs.Enable();
	}

	private void AltPerformed(InputAction.CallbackContext obj) => altInput = obj.ReadValueAsButton();
	private void JumpPerformed(InputAction.CallbackContext obj) => jumpInput = obj.ReadValueAsButton();
	private void CrouchPerformed(InputAction.CallbackContext obj) => crouchInput = obj.ReadValueAsButton();
	private void DirectionChanged(InputAction.CallbackContext obj) => directionInput = obj.ReadValue<Vector2>();
	private void AimChanged(InputAction.CallbackContext obj)
	{
		aimDeltaInput = obj.ReadValue<Vector2>();
	}

	private void OnEnable()
	{
		worldLookDir = transform.forward;

		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnDisable()
	{
		Cursor.lockState = CursorLockMode.None;
	}

	private void EnsurePhysicsMaterial(PhysicMaterial newMaterial)
	{
		if (newMaterial == lastAssignedPhysicMaterial)
			return;

		lastAssignedPhysicMaterial = newMaterial;
		foreach(Collider collider in colliders)
		{
			collider.material = newMaterial;
		}
	}


	private PhysicMaterial lastAssignedPhysicMaterial = null;
	private float maxJumpTime = 0.1f;

	private bool CanJump => BodyController.Ground.IsContacting || BodyController.Wall.IsContacting;

	private void FixedUpdate()
	{
		Rigidbody body = BodyController.Body;
		body.useGravity = false;
		Vector3 gravity = Physics.gravity;
		float speed = body.velocity.magnitude;
		Vector3 netAttractForce = BodyController.CalculateAttractForce();
		ISurfaceInfo ground = BodyController.Ground;

		// drag
		if (altInput)
			body.drag = playerConfig.JetDrag;
		else if (ground.IsContactingRaw)
			body.drag = playerConfig.WalkDrag;
		else
			body.drag = playerConfig.AirDrag;


		// jumping
		if (jumpInput)
		{
			if (CanJump)
				currentJumpTime = maxJumpTime;
		}
		else
		{
			currentJumpTime = 0;
		}
		if (currentJumpTime > 0)
		{
			currentJumpTime -= Time.fixedDeltaTime;
			body.AddForce(BodyController.Ground.Normal * playerConfig.JumpForce, ForceMode.VelocityChange);
		}

		// look correction
		Vector3 upVec = BodyController.PerceivedUp;
		Vector3 rightVec = Vector3.Cross(BodyController.PerceivedUp, worldLookDir);
		float yawAngle = aimDeltaInput.x;
		float pitchAngle = -aimDeltaInput.y;
		float angleToUp = Vector3.Angle(upVec, worldLookDir);
		float angleToDown = Vector3.Angle(-upVec, worldLookDir);
		if (pitchAngle < -angleToUp + 10)
			pitchAngle = -angleToUp + 10;
		else if (pitchAngle > angleToDown - 10)
			pitchAngle = angleToDown - 10;
		Quaternion deltaYaw = Quaternion.AngleAxis(yawAngle, upVec);
		Quaternion deltaPitch = Quaternion.AngleAxis(pitchAngle, rightVec);
		worldLookDir = deltaYaw * worldLookDir;
		worldLookDir = deltaPitch * worldLookDir;
		Quaternion lookRotation = Quaternion.LookRotation(worldLookDir, upVec);
		camNodeRoot.rotation = lookRotation;

		// rotation
		Vector3 moveDirection = Vector3.ProjectOnPlane(worldLookDir, upVec).normalized;
		moveRotation = Quaternion.LookRotation(moveDirection, upVec);

		// motion
		float animMoveSpeed;
		PhysicMaterial usedFriction = playerConfig.NoFriction;

		bool groundIsTowardsGravity = Vector3.Dot(gravity.normalized, -ground.Normal) > 0;
		if (ground.IsContacting && groundIsTowardsGravity)
		{
			gravity = Vector3.Project(gravity, -ground.Normal);
		}

		if (MathHelper.IsNearZero(directionInput))
		{
			if (ground.IsContacting && speed < playerConfig.StationaryFrictionSpeed)
				usedFriction = playerConfig.HighFriction;
			animMoveSpeed = 0;
		}
		else
		{
			body.MoveRotation(MathHelper.SmoothDamp(body.rotation, moveRotation, ref moveRotationDamp, 0.2f));
			if (ground.IsContacting)
			{
				Quaternion correction = Quaternion.FromToRotation(upVec, ground.Normal);
				moveRotation = correction * moveRotation;
			}

			moveDirection = moveRotation * new Vector3(directionInput.x, 0, directionInput.y);
			if (ground.IsContacting)
				body.AddForce(moveDirection * playerConfig.WalkMoveForce, ForceMode.Acceleration);
			else
				body.AddForce(moveDirection * playerConfig.MoveForce, ForceMode.Acceleration);
			animMoveSpeed = 1;
		}

		EnsurePhysicsMaterial(usedFriction);

		body.AddForce(gravity, ForceMode.Acceleration);
		body.AddForce(netAttractForce, ForceMode.Acceleration);

		// further processing (camera)
		MotionUpdated?.Invoke(Time.fixedDeltaTime);

		// anim
		animator.SetFloat("MoveSpeed", animMoveSpeed);
		animator.SetBool("Grounded", BodyController.Ground.IsContactingRaw);
	}


	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
			return;

		Vector3 center = BodyController.Body.worldCenterOfMass;

		// movement axis
		Gizmos.color = Color.white;
		Gizmos.DrawLine(center , center + (moveRotation * Vector3.forward) * 3);
		Gizmos.DrawLine(center, center + (moveRotation * Vector3.left));
		Gizmos.DrawLine(center, center + (moveRotation * Vector3.right));

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(center, center + worldLookDir * 3);
	}
}
