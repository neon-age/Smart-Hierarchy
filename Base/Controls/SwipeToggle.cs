using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal struct SwipeArgs
    {
        public Rect rect;
    }

    internal class SwipeToggle<T>
    {
        private static Event evt => Event.current;
        
        private static readonly int ToggleHash = "SwipeToggle".GetHashCode();
        
        private static bool wasInitialized;
        private static bool wasUndoPerformed;
        
        private Rect startRect;
        private bool targetState;
        private bool isHolding;
        private HashSet<Rect> draggedRects = new HashSet<Rect>();
        private VirtualCursor virtualCursor = new VirtualCursor();

        public SwipeToggle()
        {
            if (!wasInitialized)
            {
                wasInitialized = true;
                Undo.undoRedoPerformed += () => wasUndoPerformed = true;
            }
        }
        
        protected virtual void OnMouseDown(SwipeArgs args, T userData) {}
        protected virtual void OnStopDragging() {}

        
        public bool WillStopDragging()
        {
            return evt.rawType == EventType.MouseUp || evt.rawType == EventType.ValidateCommand;
        }
        
        public bool IsRectDragged(Rect rect)
        {
            return draggedRects.Contains(rect) || startRect == rect;
        }

        public void Cancel()
        {
            isHolding = false;
            startRect = default;
            draggedRects.Clear();
                    
            OnStopDragging();
        }

        public bool DoVerticalToggle(Rect rect, bool isActive, GUIContent content = default, GUIStyle style = default, T userData = default)
        {
            var overlapRect = new Rect(rect) { x = 0, width = Screen.width };
            return DoControl(rect, isActive, content, overlapRect, style, userData);
        }
        
        public bool DoControl(Rect rect, bool isActive, GUIContent content = default, Rect overlapRect = default, GUIStyle style = default, T userData = default)
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

            if (button == 0 && isHover && eventType == EventType.MouseDown)
            {
                GUIUtility.hotControl = controlID;

                isHolding = true;
                willToggle = true;

                targetState = isActive;

                startRect = rect;
                draggedRects.Clear();

                OnMouseDown(new SwipeArgs { rect = rect }, userData);

                evt.Use();
            }
            
            if (evt.rawType == EventType.MouseUp || wasUndoPerformed)
            {
                wasUndoPerformed = false;
                
                if (isHolding || startRect != default)
                    Cancel();

                if (isHotControl)
                {
                    GUIUtility.hotControl = 0;
                    evt.Use();
                }
            }

            var isDrag = button == 0 && isHover && isHolding && startRect != rect;

            if (isDrag && isActive == targetState && !draggedRects.Contains(rect))
            {
                // Start swiping
                draggedRects.Add(startRect);
                draggedRects.Add(rect);
                startRect = default;

                willToggle = true;
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

            return willToggle;
        }
    }
}
