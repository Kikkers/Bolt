using UnityEngine;

[CreateAssetMenu(menuName = "Bolt/Character Camera Config")]
public class ThirdpersonCameraConfig : ScriptableObject
{
	[SerializeField] private float fovBase = 60;
	[SerializeField] private float fovPowerFactor = 1.1f;
	[SerializeField] [Range(0, 1)] private float fovSmoothingFactor = 0.5f;

	[SerializeField] [Range(0, 1)] private float posSmoothingFactor = 0.5f;


	public float FovBase => fovBase;
	public float FovPowerFactor => fovPowerFactor;
	public float FovSmoothingFactor => fovSmoothingFactor;

	public float PosSmoothingFactor => posSmoothingFactor;
}
