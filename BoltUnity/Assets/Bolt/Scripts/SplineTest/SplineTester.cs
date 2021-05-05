using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SplineExperiment
{
	public class SplineTester : MonoBehaviour
	{
		[Min(1)] public int subdivisions = 4;
		[Min(0.0001f)] public float maxDistance = 0.1f;
		[Range(1, 1000)] public int maxDistanceSubdivisions = 100;

		private void OnDrawGizmos()
		{
			
			//BezierCurve curve = new BezierCurve(node1, node2);

			//Gizmos.color = Color.blue;
			//Gizmos.DrawLine(node1.Position, node1.Tangent);
			//Gizmos.DrawLine(node2.Position, node2.Tangent);

			//DrawTime(curve, subdivisions);
			//DrawDistance(curve, maxDistance, maxDistanceSubdivisions);
		}

		private static void DrawDistance(BezierCurve curve, float maxDistance, int maxSubdivisions)
		{
			Vector3 prevPos = curve.p0.Position;
			Gizmos.color = Color.yellow;

			float length = curve.Length;
			if (length / maxDistance > maxSubdivisions)
			{
				maxDistance = length / maxSubdivisions;
			}

			for(float d = maxDistance; d < length; d += maxDistance)
			{
				Vector3 newPos = curve.GetSampleByDistance(d).position;

				Gizmos.DrawLine(prevPos, newPos);
				prevPos = newPos;
			}
			Gizmos.DrawLine(prevPos, curve.p1.Position);
		}
		private static void DrawTime(BezierCurve curve, int subdivisions)
		{
			Vector3 prevPos = curve.p0.Position;

			float increment = 1.0f / subdivisions;
			float t = increment;
			for (int i = 0; i < subdivisions - 1; ++i)
			{
				Vector3 newPos = curve.GetSampleByTime(t).position;

				Gizmos.color = Color.white;
				Gizmos.DrawLine(prevPos, newPos);

				t += increment;
				prevPos = newPos;
			}
			Gizmos.DrawLine(prevPos, curve.p1.Position);
		}
	}

	public readonly struct CompactSample 
	{
		public readonly Vector3 position;
		public readonly float distance;
		public readonly float time;

		public CompactSample(Vector3 position, float distance, float time)
		{
			this.position = position;
			this.distance = distance;
			this.time = time;
		}

		/// <summary>
		/// Linearly interpolates between two curve samples.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static CompactSample Lerp(CompactSample a, CompactSample b, float t)
		{
			return new CompactSample(
				Vector3.Lerp(a.position, b.position, t),
				Mathf.Lerp(a.distance, b.distance, t),
				Mathf.Lerp(a.time, b.time, t));
		}
	}

}