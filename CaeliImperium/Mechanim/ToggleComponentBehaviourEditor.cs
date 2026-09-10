using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CaeliImperium.Mechanim
{
    [CustomEditor(typeof(ToggleComponentBehaviour))]
    public class ToggleComponentBehaviourEditor : Editor
    {
        private static string[] componentTypeNames;
        private static string[] componentTypeFullNames;
        private int selectedIndex = -1;
        private void OnEnable()
        {
            if (componentTypeNames == null)
            {
                var types = TypeCache.GetTypesDerivedFrom<Behaviour>()
                    .Where(t => !t.IsAbstract && !t.IsGenericType)
                    .OrderBy(t => t.Name)
                    .ToArray();
                componentTypeNames = types.Select(t => t.Name).ToArray();
                componentTypeFullNames = types.Select(t => t.AssemblyQualifiedName).ToArray();
            }
            SerializedProperty serializedProperty = serializedObject.FindProperty("componentTypeName");
            if (serializedProperty.stringValue.IsNullOrWhiteSpace()) return;
            selectedIndex = Array.IndexOf(componentTypeFullNames, serializedProperty.stringValue);
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty serializedProperty = serializedObject.FindProperty("componentTypeName");
            int newIndex = EditorGUILayout.Popup("Target Component", selectedIndex, componentTypeNames);
            if (newIndex != selectedIndex && newIndex >= 0 && newIndex < componentTypeFullNames.Length)
            {
                selectedIndex = newIndex;
                serializedProperty.stringValue = componentTypeFullNames[selectedIndex];
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("activityOnEnter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("activityOnExit"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
