using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class TypesPriorityGUI
    {
        private static TypeCache.TypeCollection componentTypes;
        
        private static GUIStyle miniPullDown;
        private static Texture defaultAssetIcon = EditorGUIUtility.IconContent("DefaultAsset Icon").image;

        private GUIContent tempContent = new GUIContent();
        
        private Dictionary<SerializedProperty, string> displayNames = new Dictionary<SerializedProperty, string>();
        
        public ReorderableList List { get; }
        public Action onChange;

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            componentTypes = TypeCache.GetTypesDerivedFrom<Component>();
        }

        public TypesPriorityGUI(string header, SerializedProperty property)
        {
            ScriptIcons.RetrieveFromScriptTypes(componentTypes);
            
            var typesProperty = property.FindPropertyRelative("types");
            
            List = new ReorderableList(property.serializedObject, typesProperty);
            List.elementHeight += 1;

            if (header == null)
                List.headerHeight = 1;

            List.drawHeaderCallback = rect =>
            {
                GUI.Label(rect, header);
            };

            List.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.y += 2;
                
                var item = typesProperty.GetArrayElementAtIndex(index);
                var assemblyName = item.FindPropertyRelative("assemblyQualifiedName");
                var name = item.FindPropertyRelative("fullName");
                var displayName = name.stringValue;
                var hasName = !string.IsNullOrEmpty(displayName);

                if (miniPullDown == null)
                {
                    miniPullDown = new GUIStyle("MiniPullDown")
                    {
                        fontSize = 11
                    };
                    var textColor = miniPullDown.normal.textColor;
                    textColor.a = 0.9f;
                    miniPullDown.normal.textColor = textColor;
                }

                if (hasName && !displayNames.TryGetValue(item, out displayName))
                {
                    displayName = name.stringValue.Replace("UnityEngine.", " ");
                    displayNames[item] = displayName;
                }

                tempContent.text = hasName ? displayName : "None";
                tempContent.image = ScriptIcons.GetIcon(name.stringValue);

                EditorGUIUtility.SetIconSize(new Vector2(16, 16));

                if (GUI.Button(rect, tempContent, miniPullDown))
                {
                    var position = Event.current.mousePosition;
                    position.y = rect.position.y + 35;
                    position = GUIUtility.GUIToScreenPoint(position);
                    
                    var context = new SearchWindowContext(position);

                    var popup = ScriptableObject.CreateInstance<TypeSearchPopup>();
                    popup.Initialize(componentTypes, type =>
                    {
                        name.stringValue = type.FullName;
                        assemblyName.stringValue = type.AssemblyQualifiedName;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                    SearchWindow.Open(context, popup);
                }
            };

            List.onChangedCallback += _ => SaveChanges();
            List.onReorderCallback += _ => SaveChanges();

            void SaveChanges()
            {
                property.serializedObject.ApplyModifiedProperties();
                onChange?.Invoke();
            }
        }
    }
}