using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public interface ISurfaceInfo
{
	bool IsContactingRaw { get; }
	bool IsContacting { get; }
	Vector3 Normal { get; }
	Vector3 Point { get; }
	float TimeSinceContact { get; }
}

[RequireComponent(typeof(Rigidbody))]
public class BodyController : MonoBehaviour
{
	[InlineEditor] [SerializeField] private BodyConfig bodyConfig;

	private Rigidbody body;
	public Rigidbody Body => this.GetComponentCached(ref body);

	private readonly SurfaceInfo groundInfo = new SurfaceInfo();
	private readonly SurfaceInfo wallInfo = new SurfaceInfo();
	private readonly SurfaceInfo ceilingInfo = new SurfaceInfo();

	private class SurfaceInfo : ISurfaceInfo
	{
		public bool IsContactingRaw { get; set; }
		public bool IsContacting { get; set; }
		public Vector3 Normal { get; set; }
		public Vector3 Point { get; set; }
		public float TimeSinceContact { get; set; }

		public Vector3SWA normalAverager = new Vector3SWA(4, Vector3.zero);
		public bool isUpdated;

		public void UpdateCoyoteTime(float coyoteTimeThreshold)
		{
			IsContactingRaw = TimeSinceContact == 0;
			IsContacting = TimeSinceContact <= coyoteTimeThreshold;

			TimeSinceContact += Time.fixedDeltaTime;
		}
#if UNITY_EDITOR
		public void GizmoDraw()
		{
			if (IsContacting)
			{
				Gizmos.DrawLine(Point, Point + Normal);
			}
		}
#endif
	}

	public ISurfaceInfo Ground => groundInfo;
	public ISurfaceInfo Wall => wallInfo;
	public ISurfaceInfo Ceiling => ceilingInfo;

	/// <summary>
	/// IDEA: Velocity of the wind at the current body position, would be nice if this can be changed according to vector field
	/// </summary>
	public Vector3 WindVelocity { get; set; }

	public Vector3 PerceivedUp => transform.up;

	private void OnCollisionEnter(Collision collision)
	{
		ParseContacts(collision);
	}

	private void OnCollisionStay(Collision collision)
	{
		ParseContacts(collision);
	}

	private void ParseContacts(Collision collision)
	{
		Vector3 bodyUpVec = PerceivedUp;
		float cosGround = Mathf.Cos(bodyConfig.MaxGroundAngleDeg * Mathf.Deg2Rad);
		float cosMinWall = Mathf.Cos(bodyConfig.MinWallAngleDeg * Mathf.Deg2Rad);
		float cosMaxWall = Mathf.Cos(bodyConfig.MaxWallAngleDeg * Mathf.Deg2Rad);
		float cosCeiling = Mathf.Cos(bodyConfig.MinCeilingAngleDeg * Mathf.Deg2Rad);

		int numContacts = collision.contactCount;
		for (int i = 0; i < numContacts; ++i)
		{
			ContactPoint contact = collision.GetContact(i);
			float dot = Vector3.Dot(contact.normal, bodyUpVec);
			if (dot >= cosGround)
				UpdateSurface(groundInfo, ref contact);
			if (dot <= cosMinWall && dot >= cosMaxWall)
				UpdateSurface(wallInfo, ref contact);
			if (dot <= cosCeiling)
				UpdateSurface(ceilingInfo, ref contact);
		}
	}

	private void UpdateSurface(SurfaceInfo surfaceInfo, ref ContactPoint newContact)
	{
		if (!surfaceInfo.IsContacting)
			surfaceInfo.normalAverager.Reset(newContact.normal);
		else
			surfaceInfo.normalAverager.Push(newContact.normal);

		surfaceInfo.Point = newContact.point;
		surfaceInfo.Normal = surfaceInfo.normalAverager.Avg;
		surfaceInfo.TimeSinceContact = 0; 
	}

	private void FixedUpdate()
	{
		groundInfo.UpdateCoyoteTime(bodyConfig.CoyoteTime);
		wallInfo.UpdateCoyoteTime(bodyConfig.CoyoteTime);
		ceilingInfo.UpdateCoyoteTime(bodyConfig.CoyoteTime);
	}

	public Vector3 CalculateAttractForce()
	{
		Vector3 airVelocity = Body.velocity - WindVelocity;
		float airSpeed = airVelocity.magnitude;
		Vector3 airDirection = airVelocity / airSpeed;

		IReadOnlyList<Vector3> directions = bodyConfig.ProximitySampleDirections;
		float radius = bodyConfig.ProximityRange;
		Vector3 center = Body.worldCenterOfMass;

		Vector3 cumulativeAttract = Vector3.zero;
		foreach (Vector3 dir in directions)
		{
			Ray r = new Ray(center, dir);
			if (!Physics.Raycast(r, out RaycastHit result, radius))
				continue;

			float proximityFactor = 1 - result.distance / radius;
			float angleFactor = 1 - Mathf.Abs(Vector3.Dot(airDirection, result.normal));
			if (float.IsNaN(angleFactor))
				angleFactor = 0;

			float angleForce = bodyConfig.DownforceOverParallelAngle.Evaluate(angleFactor) 
				* bodyConfig.DownforceOverParallelAngleWeight;
			float proximityForce = bodyConfig.DownforceOverProximity.Evaluate(proximityFactor) 
				* bodyConfig.DownforceOverProximityWeight;
			float speedForce = bodyConfig.DownforceOverSpeed.Evaluate(airSpeed) 
				* bodyConfig.DownforceOverSpeedWeight;

			cumulativeAttract += dir * angleForce * speedForce * proximityForce;
		}
		return cumulativeAttract;
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
			return;

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(Body.worldCenterOfMass, Body.worldCenterOfMass + CalculateAttractForce());

		Gizmos.color = Color.green;
		groundInfo.GizmoDraw();
		Gizmos.color = Color.yellow;
		wallInfo.GizmoDraw();
		Gizmos.color = Color.red;
		ceilingInfo.GizmoDraw();
	}
}
