using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;
using PopupWindow = UnityEditor.PopupWindow;

namespace AV.Hierarchy
{
    internal class ComponentsBar
    {
        private static ComponentPopupWindow popup;

        private static Event evt => Event.current;
        private static Color32 normalColor => isProSkin ? new Color32(56, 56, 56, 255) : new Color32(200, 200, 200, 255);
        private static Color32 hoverColor => isProSkin ? new Color32(69, 69, 69, 255) : new Color32(178, 178, 178, 255);
        private static Color32 selectedColor => isProSkin ? new Color32(77, 77, 77, 255) : new Color32(174, 174, 174, 255);
        private static Color32 focusColor => isProSkin ? new Color32(44, 93, 135, 255) : new Color32(58, 114, 176, 255);
        
        public ViewItem focusedItem { get; private set; }

        private VisualElement root;
        private Component hovered;
        private Component lastTarget;
       
        private ComponentPopup dummyPopup;
        private Vector2 popupSize;
        
        private Vector2 mousePos;
        private bool showPopup = true;

        public ComponentsBar(VisualElement root)
        {
            this.root = root;
        }

        private void CreateDummyPopup(Component component)
        {
            dummyPopup?.Dispose();
            dummyPopup = new ComponentPopup(component);
            dummyPopup.style.position = Position.Absolute;
            dummyPopup.style.width = 340;
            dummyPopup.visible = false;
            
            // To avoid trash frame while popup window is being opened, we need to know it's layout ahead.
            // We use this hidden dummy popup for this.
            dummyPopup.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                popupSize = dummyPopup.layout.size;
                // Layout needs to be calculated only once.
                dummyPopup.Dispose();
            });
            
            root.Add(dummyPopup);
            root.MarkDirtyRepaint();
        }
        
        public void OnGUI(Rect rect, ViewItem viewItem, bool isHovered, bool isSelected, bool isFocused)
        {
            var components = viewItem.components.data;

            if (!popup)
                focusedItem = null;

            var areaRect = new Rect(rect) { width = 2 };
            foreach (var component in components)
            {
                if (!component.isHidden)
                    areaRect.width += 16;
            }
            areaRect.x = rect.xMax - areaRect.width;
            
            // Draw color that matches background to hide label overflow.
            // TODO: Replicate with custom label rendering
            EditorGUI.DrawRect(areaRect, isFocused ? focusColor : isSelected ? selectedColor : isHovered ? hoverColor : normalColor);

            var c = 1;
            for (var i = 0; i < components.Count; i++)
            {
                var data = components[i];
                if (data.isHidden)
                    continue;

                var content = data.content;
                var component = data.component;
                mousePos = Event.current.mousePosition;

                var buttonRect = new Rect(rect) { x = rect.xMax, width = 16 };
                buttonRect.x -= buttonRect.width * c;

                var isOn = isFocused || (popup && popup.target == component);

                ViewItemGUI.DrawIcon(buttonRect, content.image, Color.white, isOn);

                if (buttonRect.Contains(mousePos))
                {
                    if (hovered != component)
                    {
                        hovered = component;
                        CreateDummyPopup(component);
                    }

                    if (evt.button == 0)
                    {
                        // Did we just clicked?
                        if (evt.type == EventType.MouseDown)
                        {
                            evt.Use();

                            if (popup)
                                popup.Close();

                            // Show popup only if target is different.
                            if (lastTarget != component)
                            {
                                showPopup = true;
                                lastTarget = component;
                            }

                            if (showPopup)
                            {
                                showPopup = false;
                                OpenPopup();
                            }

                            showPopup = !popup;
                        }

                        if (evt.type == EventType.MouseUp)
                            evt.Use();
                    }
                }

                void OpenPopup()
                {
                    focusedItem = viewItem;

                    // Don't let popup get in the way of toolbar area.
                    buttonRect.xMax = rect.xMax + 16;

                    popup = ComponentPopupWindow.Show(component, buttonRect, popupSize);
                }

                c++;
            }
        }
    }
}