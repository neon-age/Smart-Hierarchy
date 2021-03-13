using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class ActivationToggle
    {
        private static GUIStyle shurikenToggle;

        
        internal static void DoActivationToggle(Rect rect, GameObject instance, bool isShown)
        {
            if (shurikenToggle == null)
                shurikenToggle = "ShurikenToggle";

            var style = isShown ? shurikenToggle : GUIStyle.none;
            
            if (SwipeToggle.DoVerticalToggle(rect, instance.activeSelf, style: style))
            {
                Undo.RecordObject(instance, "GameObject Set Active");
                instance.SetActive(!instance.activeSelf);
            }
        }
    }
}
