using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class RectUtils
    {
        public static Rect GetCenteredRect(Rect targetRect, Rect area)
        {
            targetRect.x = area.x + (area.width / 2) - (targetRect.width / 2);
            targetRect.y = area.y + (area.height / 2) - (targetRect.height / 2);
            return targetRect;
        }
    }
}
