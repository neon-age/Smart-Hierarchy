using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    internal class TypesPriorityGUI
    {
        private static TypeCache.TypeCollection componentTypes;
        
        private static GUIStyle miniPullDown;
        private static Texture defaultAssetIcon = IconContent("DefaultAsset Icon").image;
        private static GUIContent visibilityOn = new GUIContent(IconContent("animationvisibilitytoggleon")) { tooltip = "Type is prefered." };
        private static GUIContent visibilityOff = new GUIContent(IconContent("animationvisibilitytoggleoff")) { tooltip = "Type is ignored." };

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

            List.drawElementBackgroundCallback += (rect, index, active, focused) =>
            {
                if (active || focused)
                    EditorGUI.DrawRect(rect, isProSkin ? new Color32(77, 77, 77, 255) : new Color32(174, 174, 174, 255));
            };

            List.drawElementCallback = (rect, index, active, focused) =>
            {
                if (miniPullDown == null)
                {
                    miniPullDown = new GUIStyle("MiniPullDown") { fontSize = 11 };
                    var textColor = miniPullDown.normal.textColor;
                    textColor.a = 0.9f;
                    miniPullDown.normal.textColor = textColor;
                }

                var color = GUI.color;
                var mousePos = Event.current.mousePosition;
            
                var item = typesProperty.GetArrayElementAtIndex(index);
                
                var assemblyName = item.FindPropertyRelative("assemblyQualifiedName");
                var fullName = item.FindPropertyRelative("fullName");
                var isIgnored = item.FindPropertyRelative("isIgnored");
                
                var displayName = fullName.stringValue;
                var hasName = !string.IsNullOrEmpty(displayName);

                if (hasName && !displayNames.TryGetValue(item, out displayName))
                {
                    displayName = fullName.stringValue.Replace("UnityEngine.", " ");
                    displayNames[item] = displayName;
                }

                tempContent.text = hasName ? displayName : "None";
                tempContent.image = ScriptIcons.GetIcon(fullName.stringValue);

                SetIconSize(new Vector2(16, 16));
                
                rect.y += 2;
                
                EditorGUI.BeginChangeCheck();
                
                var visibilityRect = new Rect(rect) { width = 20 };
                var visibilityIcon = isIgnored.boolValue ? visibilityOff : visibilityOn;

                if (!isIgnored.boolValue)
                    GUI.color = new Color(1, 1, 1, 0.5f);
                
                if (GUI.Button(visibilityRect, visibilityIcon, GUIStyle.none))
                {
                    isIgnored.boolValue = !isIgnored.boolValue;
                }

                GUI.color = color;
                
                rect.xMin += visibilityRect.width;

                if (GUI.Button(rect, tempContent, miniPullDown))
                {
                    var position = Event.current.mousePosition;
                    position.y = rect.position.y + 35;
                    position = GUIUtility.GUIToScreenPoint(position);
                    
                    var context = new SearchWindowContext(position);

                    var popup = ScriptableObject.CreateInstance<TypeSearchPopup>();
                    popup.Initialize(componentTypes, type =>
                    {
                        fullName.stringValue = type.FullName;
                        assemblyName.stringValue = type.AssemblyQualifiedName;
                        SaveChanges();
                    });
                    SearchWindow.Open(context, popup);
                }
                
                // PropertyField serialization doesn't work directly, something is totally wrong..
                if (EditorGUI.EndChangeCheck())
                    SaveChanges();
            };

            List.onChangedCallback += _ => SaveChanges();
            
            void SaveChanges()
            {
                // I've spent hours trying to figure why reordering/value change didn't trigger saving.
                
                // ApplyModifiedProperties on this array doesn't work unless you insert new element into it.
                // And this stupid fix makes it work! God save me.
                
                typesProperty.InsertArrayElementAtIndex(typesProperty.arraySize - 1);
                typesProperty.serializedObject.ApplyModifiedProperties();
                typesProperty.DeleteArrayElementAtIndex(typesProperty.arraySize - 1);
                typesProperty.serializedObject.ApplyModifiedProperties();
                
                onChange?.Invoke();
            }
        }
    }
}