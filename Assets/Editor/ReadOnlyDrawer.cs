using UnityEditor;
using UnityEngine;

// Used for when we want to show something in the editor, but don't want it to be able to be modified from there.
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
	{
        GUI.enabled = false;
        EditorGUI.PropertyField( position, prop, label, true );
        GUI.enabled = true;
	}
}