using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace AV.Hierarchy
{
    internal class HoverPreview : VisualElement
    {
        private static StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("5ef573d491c5ec949a5679507e4be2a4"));
        
        private AnimBool showingPreviewAnim;
        
        private float lastHoverTime;
        private Rect lastHoverPos;
        private Vector2 lastScrollPos;
        private ViewItem lastHoverItem;

        internal ObjectPreviewContainer container = new ObjectPreviewContainer();

        public HoverPreview()
        {
            showingPreviewAnim = new AnimBool { speed = 2f };
            
            Add(container);
            container.style.opacity = 0;
        }

        public void OnItemPreview(ViewItem item)
        {
            showingPreviewAnim.target = true;

            if (lastHoverItem != item)
            {
                OnItemChange(item);
                lastHoverItem = item;
            }

            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, 1000000), MouseCursor.Zoom);

            container.editor.CreateCachedEditor(new Vector2(80, 80), item.instance);
        }

        public void Hide()
        {
            showingPreviewAnim.target = false;
        }

        private void OnItemChange(ViewItem newItem)
        {
            name = newItem.instance.name;
        }

        public void SetPosition(Vector2 localMousePosition, Rect area)
        {
            if (lastHoverItem == null)
                return;
            
            var rect = new Rect();
            var position = localMousePosition;
            var size = new Vector2();
            
            var fadeSize = new Vector2(8, 8);
            var fullSize = new Vector2(80, 80);

            size = Vector2.Lerp(fadeSize, fullSize, showingPreviewAnim.faded);
            
            if (!showingPreviewAnim.target)
            {
                size = Vector2.Lerp(fadeSize, lastHoverPos.size, showingPreviewAnim.faded);
                /*
                position = Vector2.Lerp(
                        lastHoverItem.rect.position - state.scrollPos, 
                        lastHoverPos.position - new Vector2(0, 36),
                        showingPreviewAnim.faded
                    );
                position.x -= 8;
                position.y += 32;
                */
                position = Vector2.Lerp(
                    position - new Vector2(8, 0), 
                    lastHoverPos.position,
                    showingPreviewAnim.faded
                );
            }
            else
            {
                lastHoverPos = new Rect(position, size);
            }

            rect.position = position;
            rect.size = size;
            
            rect.x += 8;
            rect.y -= 12;

            rect.x = Mathf.Clamp(rect.x, 0, area.width - 16 - size.x);
            rect.y = Mathf.Clamp(rect.y, 24, area.height - 100 - size.y);
            
            container.style.width = size.x;
            container.style.height = size.y;
            
            container.style.left = rect.x;
            container.style.top = rect.y;

            container.MarkDirtyRepaint();
        }
    }
}
