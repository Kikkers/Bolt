using System;
using UnityEngine;

namespace SplineExperiment
{
	[Serializable]
	public class BezierNode
	{
		[SerializeField] private Vector3 position;

		public event Action BaseShapeChanged;

		public BezierNode(float x, float y, float z)
		{
			position = new Vector3(x, y, z);
		}

		public BezierNode(Vector3 position)
		{
			this.position = position;
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
	}
}
