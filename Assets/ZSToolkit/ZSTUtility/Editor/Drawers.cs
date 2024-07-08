using UnityEditor;
using UnityEngine;

namespace ZSToolkit.ZSTUtility
{
    [CustomPropertyDrawer(typeof(SingleUnityLayer))]
    public class SingleUnityLayerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            EditorGUI.BeginProperty(_position, GUIContent.none, _property);
            SerializedProperty layerIndex = _property.FindPropertyRelative("m_LayerIndex");
            _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
            if (layerIndex != null)
            {
                layerIndex.intValue = EditorGUI.LayerField(_position, layerIndex.intValue);
            }
            EditorGUI.EndProperty();
        }
    }

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