using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal class HoverPreview : VisualElement
    {
        private static StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("5ef573d491c5ec949a5679507e4be2a4"));

        private Rect rect;
        private ObjectPreviewBase preview;
        private ObjectPreviewContainer container = new ObjectPreviewContainer();

        
        public HoverPreview()
        {
            Add(container);
            styleSheets.Add(styleSheet);

            visible = false;
        }

        public void OnItemPreview(ViewItem item)
        {
            visible = true;
            
            if (preview == null || preview.GetTargetType() != item.mainType)
            {
                // Target type has changed, switch preview
                if (!ObjectPreviewBase.TryGetAvailablePreview(item.mainType, out preview))
                {
                    // No preview available for such type...
                    Hide();
                    return;
                }
            }

            preview.Target = item.instance;

            if (!preview.HasPreview())
                return;

            container.preview = preview;

            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, 1000000), MouseCursor.Zoom);

            preview.RenderArea = new Rect(Vector2.zero, rect.size);
            preview.RenderPreview();
        }

        public void Hide()
        {
            visible = false;
        }

        public void SetPosition(Vector2 localMousePosition, Rect area)
        {
            rect = new Rect
            {
                position = localMousePosition,
                size = new Vector2(resolvedStyle.width, resolvedStyle.height)
            };
            
            rect.x += 8;
            rect.y -= 12;

            rect.x = Mathf.Clamp(rect.x, 0, area.width - 14 - rect.size.x);
            rect.y = Mathf.Clamp(rect.y, 24, area.height - 2 - rect.size.y);
            
            container.style.width = rect.size.x;
            container.style.height = rect.size.y;
            
            container.style.left = rect.x;
            container.style.top = rect.y;
        }
    }
}