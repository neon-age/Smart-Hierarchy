using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class ActivationToggle : SwipeToggle<GameObject>
    {
        private static GUIStyle shurikenToggle;

        private int targetDepth;
        private bool isSelectionBounded;
        private HashSet<GameObject> draggedObjects = new HashSet<GameObject>();

        internal bool IsObjectDragged(GameObject instance)
        {
            return draggedObjects.Contains(instance);
        }

        internal void DoActivationToggle(Rect rect, GameObject instance, bool isShown)
        {
            if (shurikenToggle == null)
                shurikenToggle = "ShurikenToggle";
            
            var style = isShown ? shurikenToggle : GUIStyle.none;
            
            var drawRect = new Rect(rect) { width = 16 };
            drawRect = GetCenteredRect(drawRect, drawRect);
            drawRect.y += 1;
            
            if (DoVerticalToggle(rect, instance.activeSelf, userData: instance, drawRect: drawRect, style: style))
            {
                draggedObjects.Add(instance);
                Undo.RecordObject(instance, "GameObject Set Active");
                instance.SetActive(!instance.activeSelf);
            }
        }

        protected override void OnDraw(DrawArgs args)
        {
            using (new GUIColorScope(new Color(1, 1, 1, 0.5f), !args.isSwipeValid))
                base.OnDraw(args);
        }

        protected override bool OnSwipeValidate(SwipeArgs args, GameObject instance)
        {
            if (targetState != args.isActive)
                return false;
            
            var depth = GetTransformDepth(instance.transform);
                
            if (depth != targetDepth)
                return false;

            if (isSelectionBounded && !Selection.Contains(instance))
                return false;
            
            return true;
        }

        protected override void OnMouseDown(SwipeArgs args, GameObject instance)
        {
            targetState = args.isActive;
            targetDepth = GetTransformDepth(instance.transform);

            if (Selection.gameObjects.Length > 1 && Selection.Contains(instance))
                isSelectionBounded = true;
        }

        protected override void OnStopDragging()
        {
            draggedObjects.Clear();
            isSelectionBounded = false;
        }

        private static int GetTransformDepth(Transform target)
        {
            int depth = 0;
            while (target.parent != null)
            {
                target = target.parent;
                depth++;
            }
            return depth;
        }
    }
}
