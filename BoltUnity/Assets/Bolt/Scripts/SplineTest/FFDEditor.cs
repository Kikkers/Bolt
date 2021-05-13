using UnityEngine;
using UnityEditor;
using Deform.Custom;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;

namespace DeformEditor.Custom
{
	public static class FFDNodeSelectHelper
	{
		private static bool isInitialized;

		private static bool isAnyFFDItemSelected;
		private static FFDEditNode selectedNode;
		private static FreeformDeformer selectedFFD;
		
		private static List<FreeformDeformer> pendingSelectionFFDs = new List<FreeformDeformer>();
		private static List<FFDEditNode> pendingSelectionNodes = new List<FFDEditNode>();

		public static event System.Action FFDItemsDeselected;

		public static void TryInitialize()
		{
			if (isInitialized)
				return;
			isInitialized = true;

			Selection.selectionChanged -= OnPendingSelectionChanged;
			Selection.selectionChanged += OnPendingSelectionChanged;
			EditorApplication.update -= PollSelect;
			EditorApplication.update += PollSelect;
		}

		public static void PausePollSelect()
		{
			EditorApplication.update -= PollSelect;
		}

		private static void PollSelect()
		{
			GameObject activeGO = Selection.activeGameObject;
			if (isAnyFFDItemSelected)
			{
				if (activeGO == null)
				{
					isAnyFFDItemSelected = false;
					Debug.Log("activeGO == null");
					FFDItemsDeselected?.Invoke();
				}
				else
				{
					selectedNode = activeGO.GetComponent<FFDEditNode>();
					selectedFFD = activeGO.GetComponent<FreeformDeformer>();

					if (selectedNode == null && selectedFFD == null)
					{
						isAnyFFDItemSelected = false;
						Debug.Log("selectedNode == null && selectedFFD == null");
						FFDItemsDeselected?.Invoke();
					}
				}
			}
			else
			{
				if (activeGO != null)
				{
					selectedNode = activeGO.GetComponent<FFDEditNode>();
					selectedFFD = activeGO.GetComponent<FreeformDeformer>();

					if (selectedNode != null || selectedFFD != null)
					{
						isAnyFFDItemSelected = true;
						Debug.Log("selectedNode != null || selectedFFD != null");
					}
				}
			}
		}

		private static void OnPendingSelectionChanged()
		{
			pendingSelectionFFDs.Clear();
			pendingSelectionNodes.Clear();

			Object[] allSelected = Selection.objects;
			if (allSelected.Length == 0)
				return;

			foreach (GameObject selected in allSelected)
			{
				if (selected.TryGetComponent(out FreeformDeformer ffd))
					pendingSelectionFFDs.Add(ffd);

				if (selected.TryGetComponent(out FFDEditNode ffdNode))
					pendingSelectionNodes.Add(ffdNode);
			}

			if (pendingSelectionFFDs.Count > 0 || pendingSelectionNodes.Count > 0)
			{
				foreach (var ffd in pendingSelectionFFDs)
				{
					SceneVisibilityManager.instance.DisablePicking(ffd.gameObject, false);
				}

				EditorApplication.update -= PollSelect;
				EditorApplication.update += PollSelect;
			}
		}

	}

	[CustomEditor(typeof(FreeformDeformer)), CanEditMultipleObjects]
	public class FFDEditor : Editor
	{
		private void OnSceneGUI()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
					FFDNodeSelectHelper.PausePollSelect();
			}
		}

		private void OnEnable()
		{
			FFDNodeSelectHelper.TryInitialize();

			FreeformDeformer ffd = target as FreeformDeformer;

			for (int x = 0; x < ffd.Count.x; ++x)
				for (int y = 0; y < ffd.Count.y; ++y)
					for (int z = 0; z < ffd.Count.z; ++z)
					{

						FFDEditNode node = new GameObject($"ffdnode_{x}_{y}_{z}", typeof(FFDEditNode))
						{
							hideFlags = HideFlags.DontSave
						}.GetComponent<FFDEditNode>();
						node.InitEdit(ffd, FreeformDeformer.ToIndex(ffd.Count.y, ffd.Count.z, x, y, z));
					}
		}
	}
}