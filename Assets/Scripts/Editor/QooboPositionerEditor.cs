using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(QooboPositioner))]
public class QooboPositionerEditor : Editor
{
	SerializedProperty inspectorNote;

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
		if (inspectorNote == null)
		{
			inspectorNote = serializedObject.FindProperty("inspectorNote");
		}

		if (inspectorNote != null)
		{
			EditorGUILayout.HelpBox(inspectorNote.stringValue, MessageType.Info);
		}

		DrawDefaultInspector();
		serializedObject.ApplyModifiedProperties();
	}
}


