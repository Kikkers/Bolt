using Deform.Custom;
using UnityEngine;

namespace Deform.Custom
{
	[ExecuteAlways]
	public class FreeformDeformEditNode : MonoBehaviour
	{
		public FreeformDeformer owner;
		public int index;
		public float size = 0.1f;

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawCube(transform.position, Vector3.one * size);
		}

		private void OnDrawGizmosSelected()
		{
			if (owner == null)
				return;

			owner.OnDrawGizmos();
		}

		private void Reset()
		{
			hideFlags = HideFlags.DontSave;
		}

		private void Update()
		{
			if (owner.EditNodes == null ||
				index >= owner.EditNodes.Length ||
				owner.EditNodes[index] != this ||
				!FreeformDeformer.currentlyEditedFFDs.Contains(owner))
			{
#if UNITY_EDITOR
				DestroyImmediate(gameObject);
#else
				Destroy(gameObject);
#endif
			}
			else
			{	
				owner[index] = owner.transform.InverseTransformPoint(transform.position);
			}
		}
	}
}