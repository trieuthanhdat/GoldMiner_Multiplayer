#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TD.SerializableDictionary
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>.SerializableKeyValuePair))]
    public class SerializableKeyValuePairDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            EditorGUI.indentLevel = 0;

            float keyWidth = position.width * 0.4f;
            float valueWidth = position.width * 0.6f;

            Rect keyRect = new Rect(position.x, position.y, keyWidth, position.height);
            Rect valueRect = new Rect(position.x + keyWidth, position.y, valueWidth, position.height);

            EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("key"), GUIContent.none);
            EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
#endif
