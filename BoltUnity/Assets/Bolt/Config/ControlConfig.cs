using UnityEngine;

[CreateAssetMenu(menuName = "Bolt/Control Config", order = -1000)]
public class ControlConfig : ScriptableObject
{
	[Min(0.1f)] public float MouseSensitivity = 1;
}