using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class HierarchyItemGUI
    {
        private static Event evt => Event.current;

        private static Material iconMaterial;
        
        private static readonly int IsOnID = Shader.PropertyToID("_IsOn");

        private static readonly ActivationToggle activationToggle = new ActivationToggle();

        
        public static void DoActivationToggle(this GameObjectItemBase item, Rect rect, bool isHover)
        {
            var fullWidthRect = new Rect(rect) { x = 0, width = Screen.width };
            var toggleRect = new Rect(fullWidthRect) { xMin = item.viewArgs.baseIndent };

            var isDragged = activationToggle.IsObjectDragged(item.gameObject);

            if (isDragged)
            {
                var c = GUIUtil.isProSkin ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 1);
                FastGUI.DrawRect(toggleRect, new Color(c.r, c.g, c.b, 0.0666f));
            }

            activationToggle.DoActivationToggle(toggleRect, item.gameObject, isHover || isDragged);
        }
        
        public static void DrawIcon(Rect rect, Texture2D icon, Color color, bool isOn = false)
        {
            if (evt.type != EventType.Repaint)
                return;
            if (!icon.IsNull())
                DrawIconTexture(rect, icon, color, isOn);
        }
        
        public static void DrawIconTexture(Rect position, Texture texture, Color color, bool isOn = false)
        {
            if (iconMaterial.IsNull())
                iconMaterial = new Material(Shader.Find("Hidden/Internal-IconClip"));
            
            iconMaterial.SetInt(IsOnID, isOn ? 1 : 0);
            
            FastGUI.DrawTexture(position, texture, color, iconMaterial);
        }
        
        public static int OnMouseDownEvent(Rect rect)
        {
            var evt = Event.current;
            
            var hovered = rect.Contains(evt.mousePosition);
            var mouseDown = evt.type == EventType.MouseDown;
            
            if (hovered && mouseDown)
            {
                return evt.button;
            }
            return -1;
        }
        
        public static bool OnLeftClick(Rect rect)
        {
            if (OnMouseDownEvent(rect) == 0)
            {
                evt.Use();
                return true;
            }
            return false;
        }
        
        public static bool OnMiddleClick(Rect rect)
        {
            if (OnMouseDownEvent(rect) == 0)
            {
                evt.Use();
                return true;
            }
            return false;
        }
        
        public static bool OnRightClick(Rect rect)
        {
            if (OnMouseDownEvent(rect) == 1)
            {
                evt.Use();
                return true;
            }
            return false;
        }
    }
}