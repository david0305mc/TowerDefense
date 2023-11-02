using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(MButton), true)]
public class MButtonEditor : UnityEditor.UI.ButtonEditor
{
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawMButtonProperty();
    }

    private void DrawMButtonProperty()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Addition Options", EditorStyles.boldLabel);

        ++EditorGUI.indentLevel;

        serializedObject.Update();

        --EditorGUI.indentLevel;
    }
}
