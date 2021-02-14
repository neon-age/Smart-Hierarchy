using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class ViewItemGUI
    {
        private static Material iconMaterial;
        
        private static readonly Color32 OnColor = new Color32(240, 240, 240, 255);
        
        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private static readonly int OnColorID = Shader.PropertyToID("_OnColor");
        private static readonly int IsOnID = Shader.PropertyToID("_IsOn");

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

            iconMaterial.SetColor(ColorID, color);
            iconMaterial.SetColor(OnColorID, isOn ? OnColor : (Color32)Color.white);
            iconMaterial.SetInt(IsOnID, isOn ? 1 : 0);
            EditorGUI.DrawPreviewTexture(position, texture, iconMaterial);
        }
        
        public static bool OnClick(Rect rect)
        {
            var iconRect = new Rect(rect) { width = rect.height, height = rect.height };

            var clicked = iconRect.Contains(Event.current.mousePosition) &&
                        Event.current.type == EventType.MouseDown &&
                        Event.current.button == 0;
            
            if (clicked)
            {
                Event.current.Use();
                return true;
            }
            return false;
        }
    }
}