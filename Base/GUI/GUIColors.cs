using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    [InitializeOnLoad]
    public static class GUIColors
    {
        public static readonly Color32 FocusedIcon = new Color32(240, 240, 240, 255);
        public static readonly Color32 FlatIcon;

        static GUIColors()
        {
            FlatIcon = isProSkin ? new Color32(206, 206, 206, 255) : new Color32(88, 88, 88, 255);
        }
    }
}