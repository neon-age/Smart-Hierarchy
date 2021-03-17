using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class ActivationToggle
    {
        private static GUIStyle shurikenToggle;

        private static int targetDepth;
        private static HashSet<GameObject> draggedObjects = new HashSet<GameObject>();


        internal static bool IsObjectDragged(GameObject instance)
        {
            return draggedObjects.Contains(instance);
        }
        
        internal static void DoActivationToggle(Rect rect, GameObject instance, bool isShown)
        {
            if (shurikenToggle == null)
                shurikenToggle = "ShurikenToggle";

            var evt = Event.current;
            
            if (evt.type == EventType.MouseDown)
                targetDepth = GetTransformDepth(instance.transform);
                
            if (evt.rawType == EventType.MouseUp || evt.rawType == EventType.ValidateCommand)
                draggedObjects.Clear();

            var style = isShown ? shurikenToggle : GUIStyle.none;
            
            if (SwipeToggle.DoVerticalToggle(rect, instance.activeSelf, style: style))
            {
                var depth = GetTransformDepth(instance.transform);
                
                if (depth != targetDepth)
                    return;

                draggedObjects.Add(instance);
                Undo.RecordObject(instance, "GameObject Set Active");
                instance.SetActive(!instance.activeSelf);
            }
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
