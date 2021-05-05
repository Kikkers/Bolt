using System.Collections.Generic;

namespace UnityEngine
{
	public static class TransformExtensions
	{

		public static List<T> GetComponentsInDirectChildren<T>(this Transform transform)
			where T : Component
		{
			List<T> components = new List<T>();
			transform.GetComponentsInDirectChildren(components);
			return components;
		}

		public static void GetComponentsInDirectChildren<T>(this Transform transform, List<T> outChildren)
			where T : Component
		{
			int childCount = transform.childCount;
			for (int index = 0; index < childCount; index++)
			{
				Transform child = transform.GetChild(index);
				child.GetComponents(outChildren);
			}
		}

		public static string GetPathInHierarchy(this Transform transform)
		{
			string path = transform.name;
			Transform parent = transform.parent;
			while (parent != null)
			{
				path = parent.name + "/" + path;
				parent = parent.parent;
			}
			return path;
		}
	}
}
