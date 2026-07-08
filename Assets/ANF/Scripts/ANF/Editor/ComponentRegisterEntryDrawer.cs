using ANF.GUI;
using ANF.Persistent;
using ANF.Scene;
using ANF.Utils;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace ANF.Editor
{
    [CustomPropertyDrawer(typeof(ComponentRegisterEntry<DataContainer>))]
    public class ComponentRegisterEntryDataContainerDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // use the default property height, which takes into account the expanded state
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty id = property.FindPropertyRelative("id");
            SerializedProperty data = property.FindPropertyRelative("data");

            if (!string.IsNullOrEmpty(id.stringValue))
            {
                label = new GUIContent(id.stringValue);
            }
            else if (data.managedReferenceValue is DataContainer && data.managedReferenceValue != null)
            {
                label = new GUIContent(data.managedReferenceValue.GetType().FullName);
            }
            else
            {
                label = new GUIContent("Invalid");
            }

            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    [CustomPropertyDrawer(typeof(ComponentRegisterEntry<WorldComponent>))]
    public class ComponentRegisterEntryWorldComponentDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // use the default property height, which takes into account the expanded state
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty id = property.FindPropertyRelative("id");
            SerializedProperty data = property.FindPropertyRelative("data");

            if (!string.IsNullOrEmpty(id.stringValue))
            {
                label = new GUIContent(id.stringValue);
            }
            else if (data.managedReferenceValue is WorldComponent && data.managedReferenceValue != null)
            {
                label = new GUIContent(data.managedReferenceValue.GetType().FullName);
            }
            else
            {
                label = new GUIContent("Invalid");
            }

            EditorGUI.PropertyField(position, property, label, true);
        }
    }


    [CustomPropertyDrawer(typeof(GUIRegisterEntry<GUIComponent>))]
    public class GUIRegisterEntryDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // use the default property height, which takes into account the expanded state
            return EditorGUI.GetPropertyHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty id = property.FindPropertyRelative("id");
            SerializedProperty data = property.FindPropertyRelative("data");

            if (!string.IsNullOrEmpty(id.stringValue))
            {
                label = new GUIContent(id.stringValue);
            }
            else if (data.objectReferenceValue is GUIComponent && data.objectReferenceValue != null)
            {
                label = new GUIContent(data.objectReferenceValue.GetType().FullName);
            }
            else
            {
                label = new GUIContent("Invalid");
            }

            EditorGUI.PropertyField(position, property, label, true);
        }
    }
}
