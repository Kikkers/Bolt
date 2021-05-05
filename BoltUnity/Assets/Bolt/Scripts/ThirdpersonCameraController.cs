using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Camera))]
public class ThirdpersonCameraController : MonoBehaviour
{
	[SerializeField] private TestCharacterController character;
	[InlineEditor]
	[SerializeField] private ThirdpersonCameraConfig camConfig;

	private Camera cam;
	private Transform trackedCamNode;
	private float lastSpeed;

	private void Awake()
	{
		cam = GetComponent<Camera>();

		Assert.IsNotNull(character);
		character.MotionUpdated += OnCharacterMotionUpdated;
	}

	private void Start()
	{
		trackedCamNode = character.CamNodes[character.CamNodes.Count - 1];
	}

	private void OnDestroy()
	{
		character.MotionUpdated -= OnCharacterMotionUpdated;
	}

	private float fovDampVelocity;
	private Vector3 posDampVelocity;

	private void OnCharacterMotionUpdated(float deltaTime)
	{
		BodyController bodyController = character.BodyController;
		float speed = bodyController.Body.velocity.magnitude;

		// fov smoothing
		float targetFov = camConfig.FovBase * Mathf.Pow(camConfig.FovPowerFactor, speed);
		cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFov, ref fovDampVelocity, camConfig.FovSmoothingFactor);

		transform.position = Vector3.SmoothDamp(transform.position, trackedCamNode.position, ref posDampVelocity, camConfig.PosSmoothingFactor);
		transform.rotation = character.LookRoot.rotation;
	}

}
