using UnityEngine;
using UnityEditor;
using Deform.Custom;
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
	public class FFDEditor : DeformerEditor
	{
		//private void OnSceneGUI()
		//{
		//	if (Event.current.type == EventType.MouseDown)
		//	{
		//		if (Event.current.button == 0)
		//			FFDNodeSelectHelper.PausePollSelect();
		//	}
		//}

		private static class Content
		{
			public static readonly GUIContent Factor = DeformEditorGUIUtility.DefaultContent.Factor;
			public static readonly GUIContent Count = new GUIContent(text: "Count", tooltip: "FFD dimensions.");
			public static readonly GUIContent Limited = new GUIContent(text: "Limited", tooltip: "Is the deformation limited to the area.");
			public static readonly GUIContent EditingLabel = new GUIContent(text: "Edit Points");

			public static readonly GUIContent EditingIcon = EditorGUIUtility.IconContent("EditCollider");

			public static readonly GUIStyle EditButtonStyle =
				new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).button) { padding = new RectOffset(15, 15, 5, 5) };
		}

		private class Properties
		{
			public SerializedProperty Factor;
			public SerializedProperty Count;
			public SerializedProperty Limited;

			public Properties(SerializedObject obj)
			{
				Factor = obj.FindProperty("factor");
				Count = obj.FindProperty("count");
				Limited = obj.FindProperty("limited");
			}
		}

		private Properties properties;

		private bool editingControlPoints;
		private FreeformDeformer ffd;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.UpdateIfRequiredOrScript();

			EditorGUILayout.PropertyField(properties.Factor, Content.Factor);
			EditorGUILayout.PropertyField(properties.Count, Content.Count);

			serializedObject.ApplyModifiedProperties();

			EditorApplication.QueuePlayerLoopUpdate();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			bool newEditingControlPoints = GUILayout.Toggle(
				editingControlPoints,
				Content.EditingLabel,
				Content.EditButtonStyle, 
				GUILayout.ExpandWidth(false));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			if (editingControlPoints != newEditingControlPoints)
			{
				editingControlPoints = newEditingControlPoints;
				if (editingControlPoints)
					Debug.Log("TODO: edit GUI");
				else
					Debug.Log("TODO: cleanup edit GUI");
			}
		}

		[DrawGizmo(GizmoType.NotInSelectionHierarchy)]
		internal static void DrawHandlesNotSelected(FreeformDeformer ffd, GizmoType gizmoType)
		{
			Handles.color = Color.white;
			DrawHandles(ffd);
		}

		[DrawGizmo(GizmoType.Active)]
		internal static void DrawHandlesSelected(FreeformDeformer ffd, GizmoType gizmoType)
		{
			Handles.color = Color.yellow;
			DrawHandles(ffd);
		}

		internal static void DrawHandles(FreeformDeformer ffd)
		{
			Handles.matrix = ffd.transform.localToWorldMatrix;
			for (int x = 0; x < ffd.Count.x; ++x)
			{
				for (int y = 0; y < ffd.Count.y; ++y)
				{
					for (int z = 0; z < ffd.Count.z; ++z)
					{
						if (x + 1 < ffd.Count.x) Handles.DrawLine(ffd[x, y, z], ffd[x + 1, y, z]);
						if (y + 1 < ffd.Count.y) Handles.DrawLine(ffd[x, y, z], ffd[x, y + 1, z]);
						if (z + 1 < ffd.Count.z) Handles.DrawLine(ffd[x, y, z], ffd[x, y, z + 1]);
					}
				}
			}
			Handles.DrawWireCube(Vector3.one * 0.5f, Vector3.one);
		}

		public override void OnSceneGUI()
		{
			base.OnSceneGUI();

		}

		protected override void OnEnable()
		{
			base.OnEnable();

			properties = new Properties(serializedObject);
			ffd = target as FreeformDeformer;

			for (int x = 0; x < ffd.Count.x; ++x)
				for (int y = 0; y < ffd.Count.y; ++y)
					for (int z = 0; z < ffd.Count.z; ++z)
					{

						FFDEditNode node = new GameObject($"ffdnode_{x}_{y}_{z}", typeof(FFDEditNode))
						{
							hideFlags = HideFlags.DontSave
						}.GetComponent<FFDEditNode>();
						node.InitEdit(ffd, x, y, z); 
					}
		}
	}
}