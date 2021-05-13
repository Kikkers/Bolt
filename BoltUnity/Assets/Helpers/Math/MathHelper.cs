using Unity.Mathematics;
using UnityEngine;

namespace Helpers
{
	public static class MathHelper
	{
		public static bool IsNearZero(Vector3 vector, float threshold = 0.0001f) => vector.sqrMagnitude < threshold;
		public static bool IsNearZero(Vector2 vector, float threshold = 0.0001f) => vector.sqrMagnitude < threshold;

		public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
		{
			if (Time.deltaTime < Mathf.Epsilon) return rot;
			// account for double-cover
			float dot = Quaternion.Dot(rot, target);
			float multi = dot > 0f ? 1f : -1f;
			target.x *= multi;
			target.y *= multi;
			target.z *= multi;
			target.w *= multi;
			// smooth damp (nlerp approx)
			var Result = new Vector4(
				Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
				Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
				Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
				Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
			).normalized;

			// ensure deriv is tangent
			var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
			deriv.x -= derivError.x;
			deriv.y -= derivError.y;
			deriv.z -= derivError.z;
			deriv.w -= derivError.w;

			return new Quaternion(Result.x, Result.y, Result.z, Result.w);
		}


		public static float3 QuadBezier(float3 p0, float3 p1, float3 p2, float t)
		{
			float tInv = 1f - t;
			return
				p0 * (tInv * tInv) +
				p1 * (2f * tInv * t) +
				p2 * (t * t);
		}

		public static float3 CubicBezier(float3 p0, float3 p1, float3 p2, float3 p3, float t)
		{
			float tInv = 1f - t;
			float tInv2 = tInv * tInv;
			float t2 = t * t;
			return
				p0 * (tInv2 * tInv) +
				p1 * (3f * tInv2 * t) +
				p2 * (3f * tInv * t2) +
				p3 * (t2 * t);
		}
	}
}