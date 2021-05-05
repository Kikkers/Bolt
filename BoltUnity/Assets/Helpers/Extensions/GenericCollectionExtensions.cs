namespace System.Collections.Generic
{
	public static class GenericCollectionExtensions
	{
		public static bool GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue value)
			where TValue : new()
		{
			bool existed = dictionary.TryGetValue(key, out value);
			if (!existed)
			{
				dictionary[key] = value = new TValue();
			}
			return existed;
		}
	}
}
