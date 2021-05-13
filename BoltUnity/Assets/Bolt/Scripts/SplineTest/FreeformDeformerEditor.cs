using UnityEngine;
using UnityEditor;
using Deform.Custom;

namespace DeformEditor.Custom
{
	[CustomEditor(typeof(FreeformDeformer)), CanEditMultipleObjects]
	public class FreeformDeformerEditor : DeformerEditor
	{
		private static class Content
		{
			public static readonly GUIContent Points = new GUIContent(text: "Points", tooltip: "WIP.");
			public static readonly GUIContent Axis = DeformEditorGUIUtility.DefaultContent.Axis;
		}

		private class Properties
		{
			public SerializedProperty Points;
			public SerializedProperty Axis;

			public Properties(SerializedObject obj)
			{
				Points = obj.FindProperty("points");
				Axis = obj.FindProperty("axis");
			}
		}

		static void OnSelectionChanged()
		{
			Object[] allSelected = Selection.objects;
			if (allSelected.Length == 0)
				return;

			FreeformDeformer.currentlyEditedFFDs.Clear();

			foreach (Object selected in allSelected)
			{
				GameObject selectedGO = selected as GameObject;
				if (selectedGO == null)
					continue;

				if (selectedGO.TryGetComponent(out FreeformDeformer ffd))
				{
					FreeformDeformer.currentlyEditedFFDs.Add(ffd);
				}

				if (selectedGO.TryGetComponent(out FreeformDeformEditNode ffdNode))
				{
					FreeformDeformer.currentlyEditedFFDs.Add(ffdNode.owner);
				}
			}

			if (FreeformDeformer.currentlyEditedFFDs.Count == 0)
				Selection.selectionChanged -= OnSelectionChanged;
		}

		private Properties properties;

		protected override void OnEnable()
		{
			base.OnEnable();

			properties = new Properties(serializedObject);

			Selection.selectionChanged -= OnSelectionChanged;
			Selection.selectionChanged += OnSelectionChanged;

			FreeformDeformer ffd = target as FreeformDeformer;

			for (int i = 0; i < FreeformDeformer.count3d; ++i)
			{
				GameObject obj = new GameObject("ffdnode" + i, typeof(FreeformDeformEditNode))
				{
					hideFlags = HideFlags.DontSave
				};
				obj.transform.SetParent(ffd.transform);
				obj.transform.localPosition = ffd[i];
				FreeformDeformEditNode node = obj.GetComponent<FreeformDeformEditNode>();
				node.index = i;
				node.owner = ffd;
				ffd.EditNodes[i] = node;
			}
		}

		//private void OnSceneGUI()
		//{
		//
		//	//HandleUtility.PickRectObjects()
		//}
	}
}