using DeformEditor.Custom;
using UnityEngine;

namespace Deform.Custom
{
	[ExecuteAlways]
	public class FFDEditNode : MonoBehaviour
	{
		public FreeformDeformer owner;
		public int index { get; set; }
		public float size { get; set; } = 0.1f;

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

#if UNITY_EDITOR
		private void Update()
		{
			if (owner == null)
			{
				DestroyImmediate(gameObject);
				return;
			}

			owner[index] = owner.transform.InverseTransformPoint(transform.position);
		}

		public void InitEdit(FreeformDeformer owner, int index)
		{
			this.owner = owner;
			this.index = index;

			while (true)
			{
				Transform oldNode = owner.transform.Find(name);
				if (oldNode == null)
					break;
				DestroyImmediate(oldNode.gameObject);
			}

			transform.SetParent(owner.transform);
			transform.localPosition = owner[index];

			owner.NumControlPointsChanged += Cleanup;

			FFDNodeSelectHelper.FFDItemsDeselected += Cleanup;
		}

		private void OnDestroy()
		{
			FFDNodeSelectHelper.FFDItemsDeselected -= Cleanup;
			if (owner != null)
			{
				owner.NumControlPointsChanged -= Cleanup;
				UnityEditor.SceneVisibilityManager.instance.EnablePicking(owner.gameObject, true);
			}
		}

		private void Cleanup()
		{
			if (gameObject != null)
				DestroyImmediate(gameObject);
		}
#endif
	}
}
