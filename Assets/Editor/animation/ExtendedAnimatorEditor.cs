using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Animator))]
public class AnimatorExtendedEditor : Editor
{
    private Editor builtinEditor;

    private void OnEnable()
    {
        var type = Type.GetType("UnityEditor.AnimatorInspector, UnityEditor");
        if (type != null)
            builtinEditor = CreateEditor(targets, type);
    }

    public override void OnInspectorGUI()
    {
        builtinEditor?.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animation Event Bridge", EditorStyles.boldLabel);

        var animator = (Animator)target;
        var bridge = animator.GetComponent<AnimatorEventBridge>();

        if (bridge == null)
        {
            if (GUILayout.Button("Add Event Bridge"))
            {
                bridge = animator.gameObject.AddComponent<AnimatorEventBridge>();
            }
            return;
        }

        SerializedObject so = new SerializedObject(bridge);
        so.Update();

        EditorGUILayout.PropertyField(so.FindProperty("events"), true);

        so.ApplyModifiedProperties();

        EditorGUILayout.HelpBox(
            "Use Animation Event -> InvokeEvent(int index). " +
            "The int selects an element from this list.",
            MessageType.Info);
    }

    private void OnDisable()
    {
        if (builtinEditor != null)
            DestroyImmediate(builtinEditor);
    }
}