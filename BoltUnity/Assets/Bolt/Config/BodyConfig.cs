using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Bolt/Character Body Config")]
public class BodyConfig : ScriptableObject
{
	[SerializeField] [Min(0)] private float coyoteTime = 0.1f;
	[Space]
	[SerializeField] [Range(0, 180)] private float groundAngleInterval = 45;
	[SerializeField] [MinMaxSlider(0, 180, true)] private Vector2 wallAngleInterval = new Vector2(45, 100);
	[SerializeField] [Range(0, 180)] private float ceilingAngleInterval = 100;
	[Space]
	public AnimationCurve DownforceOverSpeed = AnimationCurve.Linear(0, 0, 1, 1);
	public AnimationCurve DownforceOverParallelAngle = AnimationCurve.Linear(0, 0, 1, 1);
	public AnimationCurve DownforceOverProximity = AnimationCurve.Linear(0, 0, 1, 1);
	[Range(0, 1)] public float DownforceOverSpeedWeight = 1;
	[Range(0, 1)] public float DownforceOverParallelAngleWeight = 1;
	[Range(0, 1)] public float DownforceOverProximityWeight = 1;
	[Space]
	[SerializeField] [Min(8)] private int proximitySamples = 150;
	public float ProximityRange = 4;
	
	private Vector3[] directions = new Vector3[0];

	public IReadOnlyList<Vector3> ProximitySampleDirections
	{
		get
		{
			if (proximitySamples != directions.Length)
				directions = Codeglue.GeometryHelper.GenerateDirections(proximitySamples);
			return directions;
		}
	}

	public float CoyoteTime => coyoteTime;

	public float MaxGroundAngleDeg => groundAngleInterval;
	public float MinWallAngleDeg => wallAngleInterval.x;
	public float MaxWallAngleDeg => wallAngleInterval.y;
	public float MinCeilingAngleDeg => ceilingAngleInterval;
}
