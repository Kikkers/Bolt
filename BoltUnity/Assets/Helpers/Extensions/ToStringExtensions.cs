using UnityEngine;

public static class ToStringExtensions
{
	public static string ToStringExt(this Vector3 vector)
	{
		return $"({vector.x}, {vector.y}, {vector.z})";
	}
}
