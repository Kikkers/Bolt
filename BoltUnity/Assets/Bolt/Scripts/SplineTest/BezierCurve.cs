using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SplineExperiment
{
	public class BezierCurve
	{
		private const int SEGMENTS = 20;

		private readonly List<CompactSample> samples = new List<CompactSample>();

		public readonly BezierNode p0, p1, p2, p3;

		public float Length { get; private set; }

		public BezierCurve(
			BezierNode p0,
			BezierNode p1,
			BezierNode p2,
			BezierNode p3)
		{
			this.p0 = p0;
			this.p1 = p1;
			this.p2 = p2;
			this.p3 = p3;
			p0.BaseShapeChanged += RecomputeSamples;
			p1.BaseShapeChanged += RecomputeSamples;
			p2.BaseShapeChanged += RecomputeSamples;
			p3.BaseShapeChanged += RecomputeSamples;
			RecomputeSamples();
		}

		public Vector3 GetPosition(float t)
		{
			float tInv = 1f - t;
			float tInv2 = tInv * tInv;
			float t2 = t * t;
			return
				p0.Position * (tInv2 * tInv) +
				p1.Position * (3f * tInv2 * t) +
				p2.Position * (3f * tInv * t2) +
				p3.Position * (t2 * t);
		}

		private void RecomputeSamples()
		{
			samples.Clear();
			Length = 0;

			int numSegments = SEGMENTS;

			Vector3 prevPos = p0.Position;
			samples.Add(new CompactSample(prevPos, 0, 0));

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
				samples.Add(new CompactSample(nextPos, Length, t));

				t += increment;
				prevPos = nextPos;
			}

			nextPos = p1.Position;
			delta = nextPos - prevPos;
			stepDistance = delta.magnitude;

			Length += stepDistance;
			samples.Add(new CompactSample(nextPos, Length, 1));
		}

		private struct TimeCompare : IComparer<CompactSample>
		{
			public int Compare(CompactSample x, CompactSample y)
			{
				if (x.time < y.time)
					return -1;
				else if (x.time > y.time)
					return 1;
				return 0;
			}
		}

		private struct DistanceCompare : IComparer<CompactSample>
		{
			public int Compare(CompactSample x, CompactSample y)
			{
				if (x.distance < y.distance)
					return -1;
				else if (x.distance > y.distance)
					return 1;
				return 0;
			}
		}

		public CompactSample GetSampleByTime(float time)
		{
			int index = samples.BinarySearch(
				new CompactSample(Vector3.zero, 0, time),
				new TimeCompare());

			if (index >= 0)
				return samples[index];

			index = ~index;
			CompactSample prev = samples[index - 1];
			CompactSample next = samples[index];

			float t = (time - prev.time) / (next.time - prev.time);

			return CompactSample.Lerp(prev, next, t);
		}

		public CompactSample GetSampleByDistance(float distance)
		{
			int index = samples.BinarySearch(
				new CompactSample(Vector3.zero, distance, 0),
				new DistanceCompare());

			if (index >= 0)
				return samples[index];

			index = ~index;
			CompactSample prev = samples[index - 1];
			CompactSample next = samples[index];

			float t = (distance - prev.distance) / (next.distance - prev.distance);

			return CompactSample.Lerp(prev, next, t);
		}

		public CompactSample GetProjectionSample(Vector3 pointToProject)
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
			CompactSample previous, next;
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
			var result = CompactSample.Lerp(previous, next, rate);
			return result;
		}
	}
}
