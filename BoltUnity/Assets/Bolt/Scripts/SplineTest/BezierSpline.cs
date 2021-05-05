using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace SplineExperiment
{
	public class BezierSpline : MonoBehaviour
	{
		[SerializeField] private List<BezierNode> nodes;

		[SerializeField] private Color editorColor = Color.white;
		[SerializeField] [Range(1, 10)] private float editorThickness = 3;

		private readonly List<BezierCurve> curves = new List<BezierCurve>();

		public IReadOnlyList<BezierNode> Nodes => nodes;
		public IReadOnlyList<BezierCurve> Curves => curves;

		public Color EditorColor => editorColor;
		public float EditorThickness => editorThickness;

		private void Reset()
		{
			if (nodes == null || nodes.Count < 4)
			{
				nodes = new List<BezierNode>
				{
					new BezierNode(0, 0, 0),
					new BezierNode(0, 1, 0),
					new BezierNode(1, 1, 0),
					new BezierNode(1, 1, 1)
				};

			}
		}

		internal void Refresh()
		{
			curves.Clear();
			curves.Add(new BezierCurve(
				nodes[0],
				nodes[1],
				nodes[2],
				nodes[3]));
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(BezierSpline))]
	public class BezierSplineEditor : OdinEditor
	{
		public enum EditMode
		{
			None,
			Add,
			Delete,
			Move,
			Rotate,
			Scale,
			Misc,
		}

		private EditMode editMode;
		private int editedNodeIndex = 0;

		protected override void OnEnable()
		{
			base.OnEnable();

			BezierSpline spline = target as BezierSpline;
			spline.Refresh();
			editedNodeIndex = 0;
			editMode = EditMode.None;

			ButtonIcon = EditorGUIUtility.IconContent("Button Icon");
			SettingsIcon = EditorGUIUtility.IconContent("SettingsIcon");
			AddIcon = EditorGUIUtility.IconContent("Toolbar Plus");
			RemoveIcon = EditorGUIUtility.IconContent("Toolbar Minus");
			MoveIcon = EditorGUIUtility.IconContent("MoveTool");
			RotateIcon = EditorGUIUtility.IconContent("RotateTool");
			ScaleIcon = EditorGUIUtility.IconContent("ScaleTool");
			NextIcon = EditorGUIUtility.IconContent("tab_next");
			PrevIcon = EditorGUIUtility.IconContent("tab_prev");

			SceneButtonStyle = new GUIStyle();
			SceneButtonStyle.normal.background = Texture2D.whiteTexture;
			SceneButtonStyle.hover.background = Texture2D.linearGrayTexture;
		}

		private void OnSceneGUI()
		{
			BezierSpline spline = target as BezierSpline;

			Transform splineTransform = spline.transform;

			if (Selection.activeGameObject == spline.gameObject)
			{
			}
			else
				return;

			foreach (BezierCurve curve in spline.Curves)
			{
				Handles.DrawBezier(
					splineTransform.TransformPoint(curve.p0.Position), 
					splineTransform.TransformPoint(curve.p3.Position), 
					splineTransform.TransformPoint(curve.p1.Position),
					splineTransform.TransformPoint(curve.p2.Position), 
					spline.EditorColor, Texture2D.whiteTexture, spline.EditorThickness);
			}

			Handles.BeginGUI();
			foreach (BezierNode node in spline.Nodes)
			{
				Vector3 guiPos = HandleUtility.WorldToGUIPoint(splineTransform.TransformPoint(node.Position));
				
				Button(guiPos, Color.white);
			}
			Handles.EndGUI();




		}

		private float buttonSize = 20;

		bool Button(Vector2 position, Color color)
		{
			GUI.color = color;
			return GUI.Button(
				new Rect(
					position - new Vector2(buttonSize / 2, buttonSize / 2), 
					new Vector2(buttonSize, buttonSize)), 
				GUIContent.none, 
				SceneButtonStyle);
		}
		
		public override void OnInspectorGUI()
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(" "))
				editMode = EditMode.None;
			if (GUILayout.Button(AddIcon))
				editMode = EditMode.Add;
			if (GUILayout.Button(RemoveIcon))
				editMode = EditMode.Delete;
			if (GUILayout.Button(MoveIcon))
				editMode = EditMode.Move;
			if (GUILayout.Button(RotateIcon))
				editMode = EditMode.Rotate;
			if (GUILayout.Button(ScaleIcon))
				editMode = EditMode.Scale;
			if (GUILayout.Button(SettingsIcon))
				editMode = EditMode.Misc;
			GUILayout.EndHorizontal();
			GUILayout.Label("Edit mode: " + editMode);

			EditorGUILayout.Separator();

			switch (editMode)
			{
				default:
					SelectNodeGUI();
					break;
			}

			switch (editMode)
			{
				case EditMode.Move:
					MoveGUI();
					break;
			}

			EditorGUILayout.Separator();


			Undo.RecordObject(target, "Spline Inspector Changes");

			InspectorProperty editorColor = Tree.GetPropertyAtPath("editorColor");
			editorColor.Draw();
			InspectorProperty editorThickness = Tree.GetPropertyAtPath("editorThickness");
			editorThickness.Draw();

			Tree.ApplyChanges();
		}

		private void SelectNodeGUI()
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button(PrevIcon))
			{
				editedNodeIndex--;
				if (editedNodeIndex < 0)
					editedNodeIndex = (target as BezierSpline).Nodes.Count - 1;
			}
			if (GUILayout.Button(NextIcon))
			{
				editedNodeIndex++;
				if (editedNodeIndex >= (target as BezierSpline).Nodes.Count)
					editedNodeIndex = 0;
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("Editing node " + editedNodeIndex);

			InspectorProperty nodesProp = Tree.GetPropertyAtPath("nodes");
			InspectorProperty editedNodeProp = nodesProp.Children[editedNodeIndex].Children[0];
			editedNodeProp.Draw();
			editedNodeProp.RecordForUndo("Spline Node changes");
		}

		private void MoveGUI()
		{

		}

		//public override void OnInspectorGUI()
		//{
		//	base.OnInspectorGUI();
		//}


		private static GUIStyle SceneButtonStyle { get; set; }

		private static GUIContent ButtonIcon { get; set; }
		private static GUIContent SettingsIcon { get; set; }
		private static GUIContent AddIcon { get; set; }
		private static GUIContent RemoveIcon { get; set; }
		private static GUIContent MoveIcon { get; set; }
		private static GUIContent RotateIcon { get; set; }
		private static GUIContent ScaleIcon { get; set; }
		private static GUIContent NextIcon { get; set; }
		private static GUIContent PrevIcon { get; set; }

	}
#endif
}
