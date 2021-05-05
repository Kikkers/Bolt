using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugGUI : MonoBehaviour
{
	private const string joinText = ": ";
	private const int minFramesDrawn = 10;

	private readonly Dictionary<string, TempObject> writeOnceObjs = new Dictionary<string, TempObject>();
	private readonly List<string> oldWriteOnce = new List<string>();

	private readonly Dictionary<string, object> monitorObjs = new Dictionary<string, object>();

	private static DebugGUI instance;
	private static DebugGUI Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<DebugGUI>();
				if (instance == null)
				{
					GameObject obj = new GameObject("_DebugGUI", typeof(DebugGUI))
					{
						hideFlags = HideFlags.HideAndDontSave
					};
					instance = obj.GetComponent<DebugGUI>();
				}
			}
			return instance;
		}
	}

	public static void Write(string label)
	{
		Instance.writeOnceObjs.GetOrCreate(label, out TempObject obj);
		obj.framesDrawn = 0;
		obj.strValue = label;
	}

	public static void Write(string label, object value)
	{
		Instance.writeOnceObjs.GetOrCreate(label, out TempObject obj);
		obj.framesDrawn = 0;
		obj.strValue = label + joinText + value.ToString();
	}

	public static void Monitor<T>(string label, Func<T> getter)
	{
		Instance.monitorObjs[label] = new MonitorObject<T>(label, getter);
	}

	public static void UnMonitor(string label)
	{
		Instance.monitorObjs.Remove(label);
	}

	private class MonitorObject<T>
	{
		private readonly string label;
		private readonly Func<T> getter;

		public MonitorObject(string label, Func<T> getter)
		{
			this.label = label;
			this.getter = getter;
		}

		public override string ToString()
		{
			return label + joinText + getter.Invoke().ToString();
		}
	}

	private class TempObject
	{
		public int framesDrawn;
		public string strValue;
	}

	private void OnGUI()
	{
		if (!Debug.isDebugBuild)
			return;

		GUI.contentColor = Color.black;

		GUILayout.Label("DebugGUI");
		foreach (object obj in monitorObjs)
		{
			GUILayout.Label(obj.ToString());
		}

		foreach (var pair in writeOnceObjs)
		{
			TempObject obj = pair.Value;
			obj.framesDrawn++;
			if (obj.framesDrawn > minFramesDrawn)
			{
				oldWriteOnce.Add(pair.Key);
			}
			GUILayout.Label(obj.strValue);
		}

		if (oldWriteOnce.Count > 0)
		{
			foreach (string oldKey in oldWriteOnce)
				writeOnceObjs.Remove(oldKey);
			oldWriteOnce.Clear();
		}
	}

}
