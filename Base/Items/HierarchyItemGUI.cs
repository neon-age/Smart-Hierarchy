using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class HierarchyItemGUI
    {
        private static Event evt => Event.current;
        
        private static Material iconMaterial;
        
        private static readonly Color32 OnColor = new Color32(240, 240, 240, 255);
        
        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private static readonly int OnColorID = Shader.PropertyToID("_OnColor");
        private static readonly int IsOnID = Shader.PropertyToID("_IsOn");

        
        internal static void DoItemGUI(this HierarchyItem item, HierarchyItemArgs args)
        {
            item.DoItemGUI(args);
        }

        public static void DoActivationToggle(this GameObjectItem item, Rect rect, bool isHover)
        {
            var fullWidthRect = new Rect(rect) { x = 0, width = Screen.width };
            var toggleRect = new Rect(fullWidthRect) { x = 32 };

            var isSwiped = SwipeToggle.IsRectSwiped(toggleRect);

            if (isSwiped)
            {
                var c = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 1);
                EditorGUI.DrawRect(toggleRect, new Color(c.r, c.g, c.b, 0.0666f));
            }

            ActivationToggle.DoActivationToggle(toggleRect, item.gameObject, isHover || isSwiped);
        }
        
        public static void DrawIcon(this HierarchyItem item, Rect rect, Color color, bool isOn)
        {
            var iconRect = new Rect(rect) { width = 16, height = 16 };
            iconRect.y += (rect.height - 16) / 2;

            if (item.effectiveIcon)
                DrawIconTexture(iconRect, item.effectiveIcon, color, isOn);

            if (item.overlayIcon)
                DrawIconTexture(iconRect, item.overlayIcon, color);
        }
        
        public static void DrawIconTexture(Rect position, Texture texture, Color color, bool isOn = false)
        {
            if (iconMaterial == null)
                iconMaterial = new Material(Shader.Find("Hidden/Internal-IconClip"));

            iconMaterial.SetColor(ColorID, color);
            iconMaterial.SetColor(OnColorID, isOn ? OnColor : (Color32)Color.white);
            iconMaterial.SetInt(IsOnID, isOn ? 1 : 0);
            EditorGUI.DrawPreviewTexture(position, texture, iconMaterial);
        }
        
        public static bool OnIconClick(Rect rect)
        {
            var iconRect = new Rect(rect) { width = rect.height, height = rect.height };

            var hovered = iconRect.Contains(evt.mousePosition);
            var clicked = evt.type == EventType.MouseDown && evt.button == 0;
            
            if (hovered && clicked)
            {
                evt.Use();
                return true;
            }
            return false;
        }
    }
}