using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

/// <summary>
/// Custom editor for DebouncedButton that extends the default Button editor
/// to show our additional properties in the Inspector
/// </summary>
[CustomEditor(typeof(SingleClickButton), true)]
[CanEditMultipleObjects]
public class SingleClickButtonEditor : ButtonEditor
{
    SerializedProperty cooldownDurationProperty;
    SerializedProperty disableVisualsDuringCooldownProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        cooldownDurationProperty = serializedObject.FindProperty("cooldownDuration");
        disableVisualsDuringCooldownProperty = serializedObject.FindProperty("disableVisualsDuringCooldown");
    }

    public override void OnInspectorGUI()
    {
        // Draw the default Button inspector
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debounce Settings", EditorStyles.boldLabel);

        serializedObject.Update();
        
        // Draw our custom properties
        EditorGUILayout.PropertyField(cooldownDurationProperty, new GUIContent("Cooldown Duration", "Time in seconds before the button can be clicked again"));
        EditorGUILayout.PropertyField(disableVisualsDuringCooldownProperty, new GUIContent("Disable Visuals During Cooldown", "Whether to show the button as disabled during cooldown"));
        
        serializedObject.ApplyModifiedProperties();
    }
}