using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AV.Hierarchy
{
    internal class CollectionPopup : ObjectPopupWindow
    {
        private static Texture2D collectionsIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("6ee527fd28545e04593219b473dc26da"));

        private SerializedObject serializedObject;
        private SerializedProperty colorTagProperty;
        
        public CollectionPopup(Collection collection)
        {
            title = $"Collection";
        
            serializedObject = new SerializedObject(collection);
            colorTagProperty = serializedObject.FindProperty("colorTag");
            
            var colorTags = Enum.GetValues(typeof(ColorTag)) as ColorTag[];

            var tagsBar = new Toolbar();
            
            foreach (var tag in colorTags)
            {
                var color = ColorTags.GetColor(tag);
                var image = new VisualElement 
                { 
                    style =
                    {
                        backgroundImage = collectionsIcon,
                        unityBackgroundImageTintColor = color, 
                        width = 16, height = 16
                    }
                };
                var button = new Button { style = { unityBackgroundImageTintColor = color }, userData = tag };
                
                button.AddToClassList("highlight-button");
                button.Add(image);
                
                button.clicked += () => OnTagClick(button);
                
                tagsBar.Add(button);
            }
            
            contentContainer.Add(tagsBar);
        }

        private void OnTagClick(VisualElement sender)
        {
            var tag = (ColorTag)sender.userData;

            colorTagProperty.enumValueIndex = (int)tag;
            serializedObject.ApplyModifiedProperties();
        }
    }
}