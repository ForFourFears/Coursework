using Coursework.ScriptableObjects;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Coursework.Editor
{
    [CustomPropertyDrawer(typeof(SelectSubclassDataAttribute))]
    public class SerializeReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // БРОНЯ: Если Unity случайно прислала не полиморфную ссылку (например, сам список),
            // просто рисуем стандартно и прерываем наш кастомный код.
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);

            position.xMin += 6;
            position.xMax -= 6;
            position.yMin += 4;

            Rect typeRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            string typeName = property.managedReferenceValue != null
                ? property.managedReferenceValue.GetType().Name
                : "Null (Empty)";

            // Кнопка выбора типа
            if (EditorGUI.DropdownButton(typeRect, new GUIContent($"Type: {typeName}"), FocusType.Passive))
            {
                GenericMenu menu = new();
                var fieldType = GetFieldType();

                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => fieldType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

                menu.AddItem(new GUIContent("Null"), property.managedReferenceValue == null, () =>
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                });

                foreach (var type in types)
                {
                    menu.AddItem(new GUIContent(type.Name), typeName == type.Name, () =>
                    {
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }

            // Отрисовка полей
            if (property.managedReferenceValue != null)
            {
                Rect fieldsRect = new(position.x, position.y + EditorGUIUtility.singleLineHeight + 4, position.width, position.height);

                SerializedProperty endProperty = property.GetEndProperty();
                SerializedProperty childProperty = property.Copy();

                if (childProperty.NextVisible(true))
                {
                    float currentY = fieldsRect.y;

                    while (!SerializedProperty.EqualContents(childProperty, endProperty))
                    {
                        float propertyHeight = EditorGUI.GetPropertyHeight(childProperty, true);
                        Rect propertyRect = new(fieldsRect.x, currentY, fieldsRect.width, propertyHeight);

                        if (childProperty.isArray)
                        {
                            propertyRect.x += 15f;
                            propertyRect.width -= 15f;
                        }

                        EditorGUI.PropertyField(propertyRect, childProperty, true);

                        currentY += propertyHeight + EditorGUIUtility.standardVerticalSpacing;

                        if (!childProperty.NextVisible(false)) break;

                    }
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // БРОНЯ для высоты
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            float height = EditorGUIUtility.singleLineHeight + 8;

            if (property.managedReferenceValue != null)
            {
                SerializedProperty endProperty = property.GetEndProperty();
                SerializedProperty childProperty = property.Copy();

                if (childProperty.NextVisible(true))
                {
                    while (!SerializedProperty.EqualContents(childProperty, endProperty))
                    {
                        height += EditorGUI.GetPropertyHeight(childProperty, true) + EditorGUIUtility.standardVerticalSpacing;
                        if (!childProperty.NextVisible(false)) break;
                    }
                }
                height += 4;
            }

            return height;
        }

        private Type GetFieldType()
        {
            var fieldType = fieldInfo.FieldType;
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                return fieldType.GetGenericArguments()[0];
            }
            if (fieldType.IsArray)
            {
                return fieldType.GetElementType();
            }
            return fieldType;
        }
    }
}