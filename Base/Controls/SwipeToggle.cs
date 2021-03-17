using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class SwipeToggle
    {
        private static Event evt => Event.current;
        
        private static Rect startRect;
        private static bool targetValue;
        private static bool isHolding;
        private static HashSet<Rect> draggedRects = new HashSet<Rect>();
        private static VirtualCursor virtualCursor = new VirtualCursor();

        private static readonly int ToggleHash = "SwipeToggle".GetHashCode();

        public static bool IsRectSwiped(Rect rect)
        {
            return draggedRects.Contains(rect) || startRect == rect;
        }

        public static bool DoVerticalToggle(Rect rect, bool isActive, GUIContent content = default, GUIStyle style = default)
        {
            var overlapRect = new Rect(rect) { x = 0, width = Screen.width };
            return DoControl(rect, isActive, content, overlapRect, style);
        }
        
        public static bool DoControl(Rect rect, bool isActive, GUIContent content = default, Rect overlapRect = default, GUIStyle style = default)
        {
            if (content == default)
                content = GUIContent.none;
            
            if (overlapRect == default)
                overlapRect = rect;
            
            if (style == default)
                style = GUIStyle.none;
            
            var controlID = GUIUtility.GetControlID(ToggleHash, FocusType.Passive, rect);
            var eventType = evt.GetTypeForControl(controlID);
            var isHotControl = GUIUtility.hotControl == controlID;
            
            var toggleRect = new Rect(rect) { width = 16 };

            var button = evt.button;
            var isHover = isHolding ? virtualCursor.Overlaps(overlapRect) : toggleRect.Contains(evt.mousePosition);
            var willToggle = false;
            
            virtualCursor.UpdateMousePosition();

            if (button == 0)
            {
                if (isHover && eventType == EventType.MouseDown)
                {
                    GUIUtility.hotControl = controlID;

                    isHolding = true;
                    willToggle = true;

                    targetValue = isActive;

                    startRect = rect;
                    draggedRects.Clear();

                    evt.Use();
                }
                
                if (evt.rawType == EventType.MouseUp || evt.rawType == EventType.ValidateCommand)
                {
                    if (isHotControl)
                        GUIUtility.hotControl = 0;

                    isHolding = false;

                    startRect = default;
                    draggedRects.Clear();

                    if (isHotControl)
                        evt.Use();
                }

                var isDrag = isHover && isHolding && startRect != rect;

                if (isDrag && isActive == targetValue && !draggedRects.Contains(rect))
                {
                    // Start swiping
                    draggedRects.Add(startRect);
                    draggedRects.Add(rect);
                    startRect = default;

                    willToggle = true;
                }
            }
            
            var hasFocus = isHover && isHolding && GUIUtility.hotControl == controlID;

            var drawRect = toggleRect;
            drawRect.yMin = drawRect.center.y - toggleRect.height / 2;
            drawRect.yMax = drawRect.center.y + toggleRect.height / 2;

            if (willToggle)
                isActive = !isActive;

            if (eventType == EventType.Repaint)
            {
                if (style != GUIStyle.none || content != GUIContent.none)
                    style.Draw(drawRect, content, isHover, hasFocus, isActive, hasFocus);
            }

            if (willToggle)
                return true;

            return false;
        }
    }
}
