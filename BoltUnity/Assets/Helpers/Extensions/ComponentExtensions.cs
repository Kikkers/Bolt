namespace UnityEngine
{
	public static class ComponentExtensions
	{
		/// <summary>
		/// Gets a component that matches the cache value, and will use GetComponent and set the cache if it wasn't cached yet
		/// 
		/// Intended to be used in cases where it can't be ensured that awake is called before the referenced component is used
		/// </summary>
		public static T GetComponentCached<T>(this Component source, ref T cache)
			where T : Component
		{
			if (cache != null)
				return cache;
			cache = source.GetComponent<T>();
			return cache;
		}

		public static string GetPathInHierarchy(this Component component)
		{
			return component.transform.GetPathInHierarchy();
		}
	}
}
