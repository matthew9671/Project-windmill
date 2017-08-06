using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(controller))]
public class animationEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		controller myScript = (controller)target;
		GUILayout.Label("Diff: " + myScript.diff.ToString());
		GUILayout.Label("v: " + myScript.v.ToString());
	}
}
