using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class SwipeToggle<T>
    {
        protected struct SwipeArgs
        {
            public Rect rect;
            public bool isActive;

            public SwipeArgs(Rect rect, bool isActive)
            {
                this.rect = rect;
                this.isActive = isActive;
            }
        }
        protected struct DrawArgs
        {
            public Rect rect;
            public GUIStyle style;
            public GUIContent content;
            public bool isHover;
            public bool isFocus;
            public bool isActive;
            public bool isSwipeValid;
        }
        
        private static Event evt => Event.current;
        
        private static readonly int ToggleHash = "SwipeToggle".GetHashCode();
        
        private static bool wasInitialized;
        private static bool wasUndoPerformed;
        
        protected bool targetState;
        
        private Rect startRect;
        private bool isHolding;
        private HashSet<Rect> draggedItems = new HashSet<Rect>();
        private VirtualCursor virtualCursor = new VirtualCursor();

        
        public SwipeToggle()
        {
            if (!wasInitialized)
            {
                wasInitialized = true;
                Undo.undoRedoPerformed += () => wasUndoPerformed = true;
            }
        }

        protected virtual void OnMouseDown(SwipeArgs args, T userData)
        {
            targetState = args.isActive;
        }
        protected virtual bool OnSwipeValidate(SwipeArgs args, T userData)
        {
            return targetState == args.isActive;
        }
        protected virtual void OnStartDragging(SwipeArgs args) {}
        protected virtual void OnStopDragging() {}

        protected virtual void OnDraw(DrawArgs args)
        {
            if (args.style != GUIStyle.none || args.content != GUIContent.none)
                args.style.Draw(args.rect, args.content, args.isHover, args.isFocus, args.isActive, args.isFocus);
        }
        
        
        public bool WillStopDragging()
        {
            return evt.rawType == EventType.MouseUp || evt.rawType == EventType.ValidateCommand;
        }
        
        public bool IsRectDragged(Rect rect)
        {
            return draggedItems.Contains(rect) || startRect == rect;
        }

        public void Cancel()
        {
            isHolding = false;
            startRect = default;
            draggedItems.Clear();
                    
            OnStopDragging();
        }

        public bool DoVerticalToggle(Rect rect, bool isActive, GUIContent content = default, Rect drawRect = default, GUIStyle style = default, T userData = default)
        {
            var overlapRect = new Rect(rect) { x = 0, width = Screen.width };
            return DoControl(rect, isActive, content, overlapRect, drawRect, style, userData);
        }
        
        public bool DoControl(Rect rect, bool isActive, GUIContent content = default, 
            Rect overlapRect = default, Rect drawRect = default, GUIStyle style = default, T userData = default)
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

                startRect = rect;
                draggedItems.Clear();

                OnMouseDown(new SwipeArgs(rect, isActive), userData);

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
            var isDraggedItem = draggedItems.Contains(rect);
            var isValid = true;

            if (isDrag && !isDraggedItem)
            {
                isValid = OnSwipeValidate(new SwipeArgs(rect, isActive), userData);

                if (isValid)
                {
                    // Start swiping
                    if (startRect != default)
                    {
                        draggedItems.Add(startRect);
                        OnStartDragging(new SwipeArgs(startRect, isActive));
                        startRect = default;
                    }

                    draggedItems.Add(rect);
                    willToggle = true;
                }
            }

            var hasFocus = isHover && isHolding && GUIUtility.hotControl == controlID;

            if (drawRect == Rect.zero)
            {
                drawRect = new Rect(toggleRect) { height = 16 };
                drawRect = GetCenteredRect(drawRect, toggleRect);
            }

            if (willToggle)
                isActive = !isActive;

            if (isDraggedItem)
                isValid = true;

            if (eventType == EventType.Repaint)
            {
                OnDraw(new DrawArgs { 
                    rect = drawRect,
                    style = style, 
                    content = content, 
                    isHover = isHover, 
                    isFocus = hasFocus, 
                    isActive = isActive, 
                    isSwipeValid = isValid
                });
            }

            return willToggle && isValid;
        }
        
        protected static Rect GetCenteredRect(Rect targetRect, Rect area)
        {
            return RectUtils.GetCenteredRect(targetRect, area);
        }
    }
}
