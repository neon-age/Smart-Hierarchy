using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    public static class GUIColors
    {
        public static Color32 FlatIconColor => isProSkin ? new Color32(206, 206, 206, 255) : new Color32(88, 88, 88, 255);
    }
}