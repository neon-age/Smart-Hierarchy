using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

namespace AV.Hierarchy
{
    internal class ObjectPreviewContainer : VisualElement
    {
        private static StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("5ef573d491c5ec949a5679507e4be2a4"));
       
        public readonly GameObjectPreview editor = new GameObjectPreview();
        private IMGUIContainer guiContainer;

        public ObjectPreviewContainer()
        {
            styleSheets.Add(styleSheet);
            
            guiContainer = new IMGUIContainer(() =>
            {
                var style = resolvedStyle;
                var guiStyle = guiContainer.resolvedStyle;
                
                var size = new Vector2(style.width, style.height);

                var color = GUI.color;
                GUI.color = new Color(1, 1, 1, style.opacity * guiStyle.opacity);
                
                editor.OnPreviewGUI(new Rect(Vector2.zero, size));
                
                GUI.color = color;
            });
            
            Add(guiContainer);
            
            guiContainer.StretchToParentSize();
        }
    }
}
