using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class ViewItemGUI
    {
        private static Event evt => Event.current;
        
        private static Material iconMaterial;
        
        private static readonly Color32 OnColor = new Color32(240, 240, 240, 255);
        
        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private static readonly int OnColorID = Shader.PropertyToID("_OnColor");
        private static readonly int IsOnID = Shader.PropertyToID("_IsOn");

        private static readonly ActivationToggle activationToggle = new ActivationToggle();

        
        public static void DoItemGUI(this ViewItem item, SmartHierarchy hierarchy, Rect rect, bool isHover, bool isOn)
        {
            item.DrawIcon(rect, isOn);
            
            if (item.isCollection)
            {
                if (OnIconClick(rect))
                {
                    var collectionPopup = ObjectPopupWindow.GetPopup<CollectionPopup>();
                    if (collectionPopup == null)
                    {
                        var popup = new CollectionPopup(item.collection);

                        var scrollPos = hierarchy.state.scrollPos.y;
                        var position = new Vector2(rect.x, rect.yMax - scrollPos + 32);
                        
                        popup.ShowInsideWindow(position, hierarchy.root);
                    }
                    else
                        collectionPopup.Close();
                }
            }
            
            var fullWidthRect = new Rect(rect) { x = 0, width = Screen.width };
            var toggleRect = new Rect(fullWidthRect) { x = 32 };

            var isDragged = activationToggle.IsObjectDragged(item.instance);

            if (isDragged)
            {
                var c = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 1);
                EditorGUI.DrawRect(toggleRect, new Color(c.r, c.g, c.b, 0.0666f));
            }

            activationToggle.DoActivationToggle(toggleRect, item.instance, isHover || isDragged);
        }
        
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