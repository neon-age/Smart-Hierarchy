using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class ViewItemGUI
    {
        private static Material iconMaterial;
        private static Color32 ffffff = new Color32(255, 255, 255, 255);
        private static Color32 eeeeee = new Color32(238, 238, 238, 255);
        
        private static readonly int Color = Shader.PropertyToID("_Color");
        private static readonly int OnColor = Shader.PropertyToID("_OnColor");
        private static readonly int IsOn = Shader.PropertyToID("_IsOn");

        public static void DrawIcon(this ViewItem item, Rect rect, bool isOn)
        {
            var isCollection = item.isCollection;
            
            var iconRect = new Rect(rect) { width = 16, height = 16 };
            iconRect.y += (rect.height - 16) / 2;
            
            var color = GUI.color;

            if (isCollection && !isOn)
                color *= ColorTags.GetColor(item.collection.colorTag);
            
            var renderDisabled = item.colorCode >= 4;
            
            if (renderDisabled)
                color *= new Color(1f, 1f, 1f, 0.5f);

            if (item.effectiveIcon)
                DrawIcon(iconRect, item.effectiveIcon, color, isOn);

            if (item.overlayIcon)
                DrawIcon(iconRect, item.overlayIcon, color);
        }
        
        private static void DrawIcon(Rect position, Texture texture, Color color, bool isOn = false)
        {
            if (iconMaterial == null)
                iconMaterial = new Material(Shader.Find("Hidden/Internal-IconClip"));

            iconMaterial.SetColor(Color, color);
            iconMaterial.SetColor(OnColor, isOn ? eeeeee : ffffff);
            iconMaterial.SetInt(IsOn, isOn ? 1 : 0);
            EditorGUI.DrawPreviewTexture(position, texture, iconMaterial);
        }
        
        public static bool OnIconClick(Rect rect)
        {
            var iconRect = new Rect(rect) { width = rect.height, height = rect.height };

            return GUI.Button(iconRect, GUIContent.none, GUIStyle.none);
        }
    }
}