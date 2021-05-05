using UnityEngine;

namespace Utils
{
	/// <summary>
	/// Sliding window average Vector3
	/// </summary>
	public struct Vector3SWA
	{
		private Vector3 currentSum;
		private readonly Vector3[] values;
		private int index;

		public Vector3 Avg => currentSum / WindowSize;

		public int WindowSize => values.Length;

		public Vector3SWA(int windowSize, Vector3 initialValue)
		{
			values = new Vector3[windowSize];
			index = 0;
			currentSum = default;
			Reset(initialValue);
		}

		public void Reset(Vector3 clearValue)
		{
			for (int i = 0; i < WindowSize; ++i)
			{
				values[i] = clearValue;
			}
			currentSum = clearValue * WindowSize;
		}

		public void Push(Vector3 newValue)
		{
			currentSum -= values[index];
			currentSum += newValue;
			values[index] = newValue;
			index = (index + 1) % WindowSize;
		}
	}

}
