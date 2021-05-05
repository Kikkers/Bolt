using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SplineExperiment
{
	public class SplineWrapper : MonoBehaviour
	{
		public RotationSplineNode node1 = new RotationSplineNode(Vector3.zero, Vector3.up);
		public RotationSplineNode node2 = new RotationSplineNode(Vector3.one, Vector3.right);
		[Min(1)] public int subdivisions = 4;
		[Min(0.0001f)] public float maxDistance = 0.1f;
		[Range(1, 1000)] public int maxDistanceSubdivisions = 100;

		private void OnDrawGizmos()
		{
			BezierCurve<RotationSplineNode> curve = new BezierCurve<RotationSplineNode>(node1, node2);

			Gizmos.color = Color.blue;
			Gizmos.DrawLine(node1.Position, node1.Tangent);
			Gizmos.DrawLine(node2.Position, node2.Tangent);

			DrawTime(curve, subdivisions);
			//DrawDistance(curve, maxDistance, maxDistanceSubdivisions);
		}

		private static void DrawDistance(BezierCurve<RotationSplineNode> curve, float maxDistance, int maxSubdivisions)
		{
			Vector3 prevPos = curve.n1.Position;
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
			Gizmos.DrawLine(prevPos, curve.n2.Position);
		}
		private static void DrawTime(BezierCurve<RotationSplineNode> curve, int subdivisions)
		{
			Vector3 prevPos = curve.n1.Position;

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
			Gizmos.DrawLine(prevPos, curve.n2.Position);
		}
	}

	[Serializable]
	public class RotationSplineNode : SplineNode
	{
		[SerializeField] private Vector3 up;

		public Vector3 Up => up;

		public RotationSplineNode(Vector3 position, Vector3 tangent) : base(position, tangent) { }
	}
	
	public readonly struct BaseSample 
	{
		public readonly Vector3 position;
		public readonly float distance;
		public readonly float time;

		public BaseSample(Vector3 position, float distance, float time)
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
		public static BaseSample Lerp(BaseSample a, BaseSample b, float t)
		{
			return new BaseSample(
				Vector3.Lerp(a.position, b.position, t),
				Mathf.Lerp(a.distance, b.distance, t),
				Mathf.Lerp(a.time, b.time, t));
		}
	}

	[Serializable]
	public class SplineNode 
	{
		[SerializeField] private Vector3 position;
		[SerializeField] private Vector3 tangent;

		public event Action BaseShapeChanged;

		public SplineNode(Vector3 position, Vector3 tangent)
		{
			this.position = position;
			this.tangent = tangent;
		}

		public Vector3 Position
		{
			get { return position; }
			set
			{
				if (position.Equals(value)) 
					return;
				position = value;
				BaseShapeChanged?.Invoke();
			}
		}

		public Vector3 Tangent
		{
			get { return tangent; }
			set
			{
				if (tangent.Equals(value))
					return;
				tangent = value;
				BaseShapeChanged?.Invoke();
			}
		}

	}

	[Serializable]
	public class BezierSpline<TNode>
		where TNode : SplineNode
	{

	}

	[Serializable]
	public class BezierCurve<TNode>
		where TNode : SplineNode
	{
		private const int SEGMENTS = 20;

		private readonly List<BaseSample> samples = new List<BaseSample>();

		public TNode n1, n2;

		public float Length { get; private set; }

		public BezierCurve(TNode n1, TNode n2)
		{
			this.n1 = n1;
			this.n2 = n2;
			n1.BaseShapeChanged += RecomputeSamples;
			n2.BaseShapeChanged += RecomputeSamples;
			RecomputeSamples();
		}

		public Vector3 GetPosition(float t)
		{
			float omt = 1f - t;
			float omt2 = omt * omt;
			float t2 = t * t;
			return
				n1.Position * (omt2 * omt) +
				n1.Tangent * (3f * omt2 * t) +
				n2.Tangent * (3f * omt * t2) +
				n2.Position * (t2 * t);
		}

		private void RecomputeSamples()
		{
			samples.Clear();
			Length = 0;

			int numSegments = SEGMENTS;

			Vector3 prevPos = n1.Position;
			samples.Add(new BaseSample(prevPos, 0, 0));

			float increment = 1.0f / numSegments;
			float t = increment;

			Vector3 nextPos;
			Vector3 delta;
			float stepDistance;
			for (int i = 0; i < numSegments - 1; ++i)
			{
				nextPos = GetPosition(t);
				delta = prevPos - nextPos;
				stepDistance = delta.magnitude;

				Length += stepDistance;
				samples.Add(new BaseSample(nextPos, Length, t));

				t += increment;
				prevPos = nextPos;
			}

			nextPos = n2.Position;
			delta = nextPos - prevPos;
			stepDistance = delta.magnitude;

			Length += stepDistance;
			samples.Add(new BaseSample(nextPos, Length, 1));
		}

		private struct TimeCompare : IComparer<BaseSample>
		{
			public int Compare(BaseSample x, BaseSample y)
			{
				if (x.time < y.time)
					return -1;
				else if (x.time > y.time)
					return 1;
				return 0;
			}
		}

		private struct DistanceCompare : IComparer<BaseSample>
		{
			public int Compare(BaseSample x, BaseSample y)
			{
				if (x.distance < y.distance)
					return -1;
				else if (x.distance > y.distance)
					return 1;
				return 0;
			}
		}

		public BaseSample GetSampleByTime(float time)
		{
			Assert.IsFalse(time < 0 || time > 1, $"invalid time ({time}), must be within interval (0-1)");

			int index = samples.BinarySearch(
				new BaseSample(Vector3.zero, 0, time),
				new TimeCompare());

			if (index >= 0)
				return samples[index];

			index = ~index;
			BaseSample prev = samples[index - 1];
			BaseSample next = samples[index];

			float t = (time - prev.time) / (next.time - prev.time);

			return BaseSample.Lerp(prev, next, t);
		}

		public BaseSample GetSampleByDistance(float distance)
		{
			Assert.IsFalse(distance < 0 || distance > Length, $"invalid time ({distance}), must be within interval (0-{Length})");

			int index = samples.BinarySearch(
				new BaseSample(Vector3.zero, distance, 0),
				new DistanceCompare());

			if (index >= 0)
				return samples[index];

			index = ~index;
			BaseSample prev = samples[index - 1];
			BaseSample next = samples[index];

			float t = (distance - prev.distance) / (next.distance - prev.distance);

			return BaseSample.Lerp(prev, next, t);
		}

		public BaseSample GetProjectionSample(Vector3 pointToProject)
		{
			float minSqrDistance = float.PositiveInfinity;
			int closestIndex = -1;
			int i = 0;
			foreach (var sample in samples)
			{
				float sqrDistance = (sample.position - pointToProject).sqrMagnitude;
				if (sqrDistance < minSqrDistance)
				{
					minSqrDistance = sqrDistance;
					closestIndex = i;
				}
				i++;
			}
			BaseSample previous, next;
			if (closestIndex == 0)
			{
				previous = samples[closestIndex];
				next = samples[closestIndex + 1];
			}
			else if (closestIndex == samples.Count - 1)
			{
				previous = samples[closestIndex - 1];
				next = samples[closestIndex];
			}
			else
			{
				float toPreviousSample = (pointToProject - samples[closestIndex - 1].position).sqrMagnitude;
				float toNextSample = (pointToProject - samples[closestIndex + 1].position).sqrMagnitude;
				if (toPreviousSample < toNextSample)
				{
					previous = samples[closestIndex - 1];
					next = samples[closestIndex];
				}
				else
				{
					previous = samples[closestIndex];
					next = samples[closestIndex + 1];
				}
			}

			var onCurve = Vector3.Project(pointToProject - previous.position, next.position - previous.position) + previous.position;
			float rate = (onCurve - previous.position).sqrMagnitude / (next.position - previous.position).sqrMagnitude;
			rate = Mathf.Clamp(rate, 0, 1);
			var result = BaseSample.Lerp(previous, next, rate);
			return result;
		}
	}
}