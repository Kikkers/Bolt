using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleSampleCharacterControl : MonoBehaviour
{
    private enum ControlMode
    {
        /// <summary>
        /// Up moves the character forward, left and right turn the character gradually and down moves the character backwards
        /// </summary>
        Tank,
        /// <summary>
        /// Character freely moves in the chosen direction from the perspective of the camera
        /// </summary>
        Direct
    }

    [SerializeField] private float moveSpeed = 2;
    [SerializeField] private float turnSpeed = 200;
    [SerializeField] private float jumpForce = 4;

    [SerializeField] private Animator animator = null;
    [SerializeField] private Rigidbody rigidBody = null;

    [SerializeField] private ControlMode controlMode = ControlMode.Direct;

	private InputMaster inputs;

	private float currentV = 0;
    private float currentH = 0;

    private const float interpolation = 10;
    private const float walkScale = 0.33f;
    private const float backwardsWalkScale = 0.16f;
    private const float backwardRunScale = 0.66f;
	private const float minJumpInterval = 0.25f;

	private bool wasGrounded;
    private Vector3 currentDirection = Vector3.zero;

    private float jumpTimeStamp = 0;

    private bool jumpInput = false;
	private bool sprintInput = false;
	private Vector2 directionInput = Vector2.zero;

    private bool isGrounded;

    private readonly List<Collider> collisions = new List<Collider>();

    private void Awake()
    {
		inputs = new InputMaster();
		inputs.Character.Jump.performed += JumpPerformed;
		inputs.Character.Direction.started += DirectionChanged;
		inputs.Character.Direction.performed += DirectionChanged;
		inputs.Character.Direction.canceled += DirectionChanged;
		inputs.Character.Crouch.performed += SprintPerformed;
		inputs.Enable();
    }

	private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                if (!collisions.Contains(collision.collider))
                {
                    collisions.Add(collision.collider);
                }
                isGrounded = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true; break;
            }
        }

        if (validSurfaceNormal)
        {
            isGrounded = true;
            if (!collisions.Contains(collision.collider))
            {
                collisions.Add(collision.collider);
            }
        }
        else
        {
            if (collisions.Contains(collision.collider))
            {
                collisions.Remove(collision.collider);
            }
            if (collisions.Count == 0) { isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collisions.Contains(collision.collider))
        {
            collisions.Remove(collision.collider);
        }
        if (collisions.Count == 0) { isGrounded = false; }
    }

	private void JumpPerformed(InputAction.CallbackContext obj)
	{
		if (!jumpInput)
		{
			jumpInput = obj.ReadValueAsButton();
		}
	}

	private void SprintPerformed(InputAction.CallbackContext obj)
	{
		sprintInput = obj.ReadValueAsButton();
	}

	private void DirectionChanged(InputAction.CallbackContext obj)
	{
		directionInput = obj.ReadValue<Vector2>();
	}

	private void FixedUpdate()
    {
        animator.SetBool("Grounded", isGrounded);

        switch (controlMode)
        {
            case ControlMode.Direct:
                DirectUpdate();
                break;

            case ControlMode.Tank:
                TankUpdate();
                break;

            default:
                Debug.LogError("Unsupported state");
                break;
        }

        wasGrounded = isGrounded;
        jumpInput = false;
    }

    private void TankUpdate()
    {
		float v = directionInput.y; 
        float h = directionInput.x; 

		bool walk = sprintInput;

		if (v < 0)
        {
            if (walk) { v *= backwardsWalkScale; }
            else { v *= backwardRunScale; }
        }
        else if (walk)
        {
            v *= walkScale;
        }

        currentV = Mathf.Lerp(currentV, v, Time.deltaTime * interpolation);
        currentH = Mathf.Lerp(currentH, h, Time.deltaTime * interpolation);

        transform.position += transform.forward * currentV * moveSpeed * Time.deltaTime;
        transform.Rotate(0, currentH * turnSpeed * Time.deltaTime, 0);

        animator.SetFloat("MoveSpeed", currentV);

        JumpingAndLanding();
    }

    private void DirectUpdate()
	{
		float v = directionInput.y;
		float h = directionInput.x;

        Transform camera = Camera.main.transform;

        if (sprintInput)
        {
            v *= walkScale;
            h *= walkScale;
        }

        currentV = Mathf.Lerp(currentV, v, Time.deltaTime * interpolation);
        currentH = Mathf.Lerp(currentH, h, Time.deltaTime * interpolation);

        Vector3 direction = camera.forward * currentV + camera.right * currentH;

        float directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * directionLength;

        if (direction != Vector3.zero)
        {
            currentDirection = Vector3.Slerp(currentDirection, direction, Time.deltaTime * interpolation);

            transform.rotation = Quaternion.LookRotation(currentDirection);
            transform.position += currentDirection * moveSpeed * Time.deltaTime;

            animator.SetFloat("MoveSpeed", direction.magnitude);
        }

        JumpingAndLanding();
    }

    private void JumpingAndLanding()
    {
        bool jumpCooldownOver = (Time.time - jumpTimeStamp) >= minJumpInterval;

        if (jumpCooldownOver && isGrounded && jumpInput)
        {
            jumpTimeStamp = Time.time;
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (!wasGrounded && isGrounded)
        {
            animator.SetTrigger("Land");
        }

        if (!isGrounded && wasGrounded)
        {
            animator.SetTrigger("Jump");
        }
    }
}
