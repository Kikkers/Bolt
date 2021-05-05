
using UnityEngine;

namespace Codeglue
{
	public static class GeometryHelper
	{
		public static Vector3[] GenerateDirections(int count)
		{
			return GenerateDirectionsInternal(count, count);
		}

		public static Vector3[] GenerateDirections(int density, float amountOccupied)
		{
			int cutoff = Mathf.CeilToInt(density * Mathf.Clamp(amountOccupied, 0, 1));
			return GenerateDirectionsInternal(density, cutoff);
		}

		private static Vector3[] GenerateDirectionsInternal(int density, int cutoff)
		{
			Vector3[] directions = new Vector3[cutoff];

			float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
			float angleIncrement = Mathf.PI * 2 * goldenRatio;

			for (int i = 0; i < cutoff; i++)
			{
				float t = (float)i / density;
				float inclination = Mathf.Acos(1 - 2 * t);
				float azimuth = angleIncrement * i;

				float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
				float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
				float z = Mathf.Cos(inclination);
				directions[i] = new Vector3(x, y, z);
			}
			return directions;
		}
	}
}