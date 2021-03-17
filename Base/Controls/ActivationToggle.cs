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
            
            if (DoVerticalToggle(rect, instance.activeSelf, userData: instance, style: style))
            {
                var depth = GetTransformDepth(instance.transform);
                
                if (depth != targetDepth)
                    return;

                draggedObjects.Add(instance);
                Undo.RecordObject(instance, "GameObject Set Active");
                instance.SetActive(!instance.activeSelf);
            }
        }
        
        protected override void OnMouseDown(SwipeArgs args, GameObject instance)
        {
            targetDepth = GetTransformDepth(instance.transform);
        }

        protected override void OnStopDragging()
        {
            draggedObjects.Clear();
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
