using UnityEngine;

[CreateAssetMenu(menuName = "Bolt/Player Config", order = -1000)]
public class PlayerConfig : ScriptableObject
{
	[Min(1)] public int GrappleSpawns = 1;
	[Min(0.1f)] public float GrappleSpeed = 1;
	[Min(0.1f)] public float GrappleForce = 1;
	[Space]
	[Min(0.1f)] public float JetpackFuel = 1;
	[Min(0.1f)] public float JetpackForce = 1;
	[Space]
	[Min(0)] public float WalkDrag = 1;
	[Min(0)] public float AirDrag = 0;
	[Min(0)] public float JetDrag = 0;
	[Space]
	[Min(0)] public float WalkMoveForce = 1;
	[Min(0)] public float MoveForce = 1;
	[Min(0)] public float JumpForce = 1;
	[Space]
	[Min(0)] public float OnSurfaceCoyoteTime = 0.1f;
	[Min(0)] public float StationaryFrictionSpeed = 0.1f;
	public PhysicMaterial HighFriction;
	public PhysicMaterial NoFriction;
}