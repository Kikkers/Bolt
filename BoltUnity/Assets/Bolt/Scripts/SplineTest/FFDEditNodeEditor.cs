using Deform.Custom;
using UnityEditor;
using UnityEngine;

namespace DeformEditor.Custom
{

	[CustomEditor(typeof(FFDEditNodeEditor)), CanEditMultipleObjects]
	public class FFDEditNodeEditor : Editor
	{
		private void OnSceneGUI()
		{
			if (Event.current.type == EventType.MouseDown)
			{
				if (Event.current.button == 0)
					FFDNodeSelectHelper.PausePollSelect();
			}
		}

	}
}
