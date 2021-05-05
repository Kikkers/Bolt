using Extensions.IntFlags;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;

[RequireComponent(typeof(Rigidbody))]
public class MyCharacterController : MonoBehaviour
{
	private const int STATUS_SLIDING = 1;
	private const int STATUS_GRAPPLING = 2;
	private const int STATUS_GRAPPLETRAVELLING = 4;

	[SerializeField] private Transform camNode;
	[SerializeField] private Transform gunNozzle;
	[Header("Settings")]
	[InlineEditor] [SerializeField] private PlayerConfig settings;
	[InlineEditor] [SerializeField] private ControlConfig controlSettings;

	[Header("Prefabs")]
	[AssetsOnly] [SerializeField] private GrappleShot grappleShotPrefab;

	private readonly Inputs inputs = new Inputs();

	private Rigidbody body;
	private Collider[] colliders;
	private Vector3SWA contactNormal = new Vector3SWA(4, Vector3.zero);

	private float yaw;
	private float pitch;
	private int statusFlags;
	private float lastTimeOnSurface;

	private Vector3 lastJP;
	private Vector3 lastJump;
	private Vector3 lastGround;

	private bool IsOnSurfaceCoyoteTime => lastTimeOnSurface + settings.OnSurfaceCoyoteTime > Time.fixedTime;

	private void Awake()
	{
		Assert.IsNotNull(camNode);
		Assert.IsNotNull(gunNozzle);
		Assert.IsNotNull(grappleShotPrefab);
		Assert.IsNotNull(settings);
		Assert.IsNotNull(controlSettings);

		body = GetComponent<Rigidbody>();
		Assert.IsNotNull(body);
		colliders = GetComponentsInChildren<Collider>(true);
		Assert.IsTrue(colliders.Length > 0);

		ChangePhysicsMaterials(settings.HighFriction);
	}

	private void Reset()
	{
		body = GetComponent<Rigidbody>();
		camNode = GetComponentInChildren<Camera>().transform.parent;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!IsOnSurfaceCoyoteTime)
		{
			ContactPoint contact = collision.GetContact(0);
			contactNormal.Reset(contact.normal);
			int numContacts = collision.contactCount;
			for (int i = 1; i < numContacts; ++i)
			{
				contact = collision.GetContact(i);
				contactNormal.Push(contact.normal);
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		int numContacts = collision.contactCount;
		for(int i =0; i< numContacts; ++i)
		{
			ContactPoint contact = collision.GetContact(i);
			contactNormal.Push(contact.normal);
		}
		lastTimeOnSurface = Time.fixedTime;
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
			return;

		Vector3 origin = body.worldCenterOfMass;
		if (IsOnSurfaceCoyoteTime)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(origin, origin + contactNormal.Avg * 2);
		}

		Gizmos.color = Color.green;
		Gizmos.DrawLine(origin, origin + lastJP * 4);
		Gizmos.DrawLine(origin, origin + lastGround * 4);
		Gizmos.DrawLine(origin, origin + lastJump * 4);

	}

	private void OnGUI()
	{
		string surfaceText = IsOnSurfaceCoyoteTime ?
			Vector3.Dot(Vector3.up, contactNormal.Avg).ToString("F3") :
			"None";

		GUI.Label(new Rect(0, 0, 300, 30), "surface: " + surfaceText);
	}

	private void OnEnable()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnDisable()
	{
		Cursor.lockState = CursorLockMode.None;
	}

	private void UpdateDirectionAxis(out float axis, KeyCode positiveInput, KeyCode negativeInput, ref bool anyPressed)
	{
		bool pos = Input.GetKey(positiveInput);
		bool neg = Input.GetKey(negativeInput);
		
		if (pos && neg)
			axis = 0;
		else if (pos)
			axis = 1;
		else if (neg)
			axis = -1;
		else
			axis = 0;

		if (axis != 0)
			anyPressed = true;
	}

	private void UpdateInputs()
	{
		bool anyPressed = false;
		UpdateDirectionAxis(out float forward, KeyCode.W, KeyCode.S, ref anyPressed);
		UpdateDirectionAxis(out float strafe, KeyCode.D, KeyCode.A, ref anyPressed);
		inputs.horizontalMoveDirection = anyPressed;
		UpdateDirectionAxis(out float up, KeyCode.Space, KeyCode.LeftShift, ref anyPressed);
		inputs.anyMoveDirection = anyPressed;
		inputs.verticalMoveDirection = !inputs.horizontalMoveDirection && anyPressed;
		if (anyPressed)
			inputs.moveDirection = new Vector3(strafe, up, forward);
		else
			inputs.moveDirection = Vector3.zero;

		inputs.grapple = Input.GetMouseButton(0);
		inputs.lookDelta.x = Input.GetAxis("Mouse X");
		inputs.lookDelta.y = Input.GetAxis("Mouse Y");

		yaw += inputs.lookDelta.x * controlSettings.MouseSensitivity;
		pitch -= inputs.lookDelta.y * controlSettings.MouseSensitivity;

		camNode.localRotation = Quaternion.AngleAxis(pitch, Vector3.right);
		transform.localRotation = Quaternion.AngleAxis(yaw, Vector3.up);
	}

	private void ChangePhysicsMaterials(PhysicMaterial newMaterial)
	{
		foreach (Collider collider in colliders)
		{
			collider.sharedMaterial = newMaterial;
		}
	}

	private void Update()
	{
		UpdateInputs();

	}

	public class Inputs
	{	
		public Vector2 lookDelta;

		public bool grapple;

		public Vector3 moveDirection;
		internal bool anyMoveDirection;
		internal bool horizontalMoveDirection;
		internal bool verticalMoveDirection;
	}

	private void FixedUpdate()
	{
		bool shouldSlide = inputs.anyMoveDirection;
		if (shouldSlide != statusFlags.HasFlag(STATUS_SLIDING))
		{
			if (shouldSlide)
				ChangePhysicsMaterials(settings.NoFriction);
			else
				ChangePhysicsMaterials(settings.HighFriction);
			statusFlags.SetFlag(STATUS_SLIDING, shouldSlide);
		}

		lastGround = default;
		lastJP = default;
		lastJump = default;
		if (inputs.anyMoveDirection)
		{
			if (IsOnSurfaceCoyoteTime)
			{
				if (inputs.horizontalMoveDirection)
				{
					Quaternion rotation = Quaternion.LookRotation(transform.forward, contactNormal.Avg);
					Vector3 direction = rotation * Vector3.forward;
					body.AddForce(direction * settings.JetpackForce, ForceMode.Acceleration);
					lastGround = direction;
				}
				if (inputs.verticalMoveDirection)
				{
					lastJump = contactNormal.Avg;
					body.AddForce(contactNormal.Avg * settings.JumpForce, ForceMode.Acceleration);
				}
			}
			else
			{
				Vector3 direction = transform.TransformDirection(inputs.moveDirection.normalized);
				lastJP = direction;
				body.AddForce(direction * settings.JetpackForce, ForceMode.Acceleration);
			}
		}

		if (inputs.grapple)
		{
			TrySpawnGrapple();
		}
		else
		{
			TryResetGrapple();
		}

		
		if (statusFlags.HasFlag(STATUS_GRAPPLING))
		{
			Vector3 direction = (grapplePoint - transform.position).normalized;
			body.AddForce(direction * settings.GrappleForce, ForceMode.Acceleration);
		}
	}

	private Vector3 grapplePoint;
	private Vector3 grappleNormal;
	private float desiredGrappleDistance;
	private List<GrappleShot> currentGrappleShots = new List<GrappleShot>();

	private void TryResetGrapple()
	{
		ClearGrappleShots();
		statusFlags.SetFlag(STATUS_GRAPPLETRAVELLING | STATUS_GRAPPLING, false);
	}

	private void TrySpawnGrapple()
	{
		if (statusFlags.HasFlag(STATUS_GRAPPLING | STATUS_GRAPPLETRAVELLING))
			return;

		statusFlags.SetFlag(STATUS_GRAPPLETRAVELLING, true);

		Vector3 fwd = camNode.forward;

		for (int i = 0; i < settings.GrappleSpawns; ++i)
		{
			GrappleShot instance = Instantiate(grappleShotPrefab, gunNozzle.position, Quaternion.LookRotation(gunNozzle.forward, transform.up));
			instance.Body.AddForce(fwd * settings.GrappleSpeed, ForceMode.VelocityChange);
			instance.Collided += GrappleCollided;
			currentGrappleShots.Add(instance);
		}
	}

	private void ClearGrappleShots()
	{
		foreach (GrappleShot shot in currentGrappleShots)
		{
			Destroy(shot.gameObject);
		}
		currentGrappleShots.Clear();
	}

	private void GrappleCollided(Collision collision)
	{
		statusFlags.SetFlag(STATUS_GRAPPLING, true);
		ContactPoint contact = collision.GetContact(0);
		grapplePoint = contact.point;
		grappleNormal = contact.normal;
		desiredGrappleDistance = (grapplePoint - transform.position).magnitude;

		ClearGrappleShots();
	}
}
