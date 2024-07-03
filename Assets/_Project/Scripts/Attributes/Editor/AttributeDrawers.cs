using UnityEditor;
using UnityEngine;

namespace Fifbox.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(WithComponentAttribute))]
    public class RequireComponentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label);
            if (EditorGUI.EndChangeCheck())
            {
                GameObject gameObject = property.objectReferenceValue as GameObject;
                if (gameObject != null)
                {
                    var attributeType = (attribute as WithComponentAttribute).RequiredComponentType;
                    var component = gameObject.GetComponent(attributeType);
                    if (!component)
                    {
                        Debug.LogError($"{attributeType.Name} not found on {gameObject.name}");
                        property.objectReferenceValue = null;
                    }
                }
            }
        }
    }
}