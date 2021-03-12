using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;
using PopupWindow = UnityEditor.PopupWindow;

namespace AV.Hierarchy
{
    // TODO: Make toolbar and popups state serializable
    internal class ComponentsToolbar
    {
        static ComponentPopupWindow popupWindow;

        static Event evt => Event.current;
        static Color32 normalColor => isProSkin ? new Color32(56, 56, 56, 255) : new Color32(200, 200, 200, 255);
        static Color32 hoverColor => isProSkin ? new Color32(69, 69, 69, 255) : new Color32(178, 178, 178, 255);
        static Color32 selectedColor => isProSkin ? new Color32(77, 77, 77, 255) : new Color32(174, 174, 174, 255);
        static Color32 focusColor => isProSkin ? new Color32(44, 93, 135, 255) : new Color32(58, 114, 176, 255);
        
        public ViewItem focusedItem { get; private set; }

        VisualElement root;
        ComponentPopup dummyPopup;

        Component lastHovered;
        Component lastClicked;
        Rect lastClickedRect;
        
        Vector2 popupSize;
        
        bool showPopup;
        bool waitsForLayoutUpdate;
        
        
        public ComponentsToolbar(VisualElement root)
        {
            this.root = root;
        }

        void CreateDummyPopup(Component component)
        {
            dummyPopup?.Dispose();
            dummyPopup = new ComponentPopup(component);
            dummyPopup.style.position = Position.Absolute;
            dummyPopup.style.width = 340;
            dummyPopup.visible = false;
            
            // To avoid trash frame while popup window is being opened, we need to know it's layout ahead.
            // We use the hidden dummy popup for this.
            dummyPopup.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                popupSize = dummyPopup.layout.size;
                waitsForLayoutUpdate = false;
                
                // Layout needs to be calculated only once.
                dummyPopup.Dispose();
            });
            
            root.Add(dummyPopup);
            //dummyPopup.MarkDirtyRepaint();
           // repaintImmediately.Invoke(SmartHierarchy.active.window.actualWindow, null);
        }
        
        public void OnGUI(Rect rect, ViewItem viewItem, bool isHovered, bool isSelected, bool isFocused)
        {
            OpenHoveredPopup();
            
            void OpenHoveredPopup()
            {
                if (!showPopup || waitsForLayoutUpdate) 
                    return;
                showPopup = false;

                //var lastPopup = popup;
                //if (popup)
                //    popup.Close();

                OpenPopup(lastClickedRect, rect, lastClicked);

                //popup.onShow = lastPopup.Close;
            }

            var components = viewItem.components.data;

            if (!popupWindow)
                focusedItem = null;

            var areaRect = new Rect(rect) { width = 2 };
            foreach (var component in components)
            {
                if (!component.isHidden)
                    areaRect.width += 16;
            }
            areaRect.x = rect.xMax - areaRect.width;
            
            // Draw color that matches background to hide label overflow.
            // TODO: Hide overflow on label itself instead of overlaying color
            EditorGUI.DrawRect(areaRect, isFocused ? focusColor : isSelected ? selectedColor : isHovered ? hoverColor : normalColor);

            var c = 1;
            for (var i = 0; i < components.Count; i++)
            {
                var data = components[i];
                if (data.isHidden)
                    continue;

                var content = data.content;
                var component = data.component;
                var mousePos = Event.current.mousePosition;

                var buttonRect = new Rect(rect) { x = rect.xMax, width = 16 };
                buttonRect.x -= buttonRect.width * c;

                var isOn = isFocused || (popupWindow && popupWindow.target == component);

                ViewItemGUI.DrawIconTexture(buttonRect, content.image, Color.white, isOn);

                var overlapRect = buttonRect;

                if (overlapRect.Contains(mousePos))
                {
                    if (evt.button == 0)
                    {
                        // TODO: Delay mouse drag after popup closure
                        if (evt.type == EventType.MouseDown || evt.type == EventType.MouseDrag)
                        {
                            // Show popup only if target is different.
                            if (!popupWindow || lastClicked != component)
                            {
                                showPopup = true;
                                lastClicked = component;
                                lastClickedRect = buttonRect;

                                waitsForLayoutUpdate = true;
                                
                                CreateDummyPopup(component);

                                EditorUtility.SetDirty(SmartHierarchy.active.window.actualWindow);
                                SmartHierarchy.active.window.actualWindow.Repaint();
                                //EditorApplication.update.Invoke();
                            }
                            else if (popupWindow)
                            {
                                if (evt.type == EventType.MouseDown)
                                    popupWindow.Close();
                            }

                            evt.Use();
                        }

                        if (evt.type == EventType.MouseUp)
                        {
                            evt.Use();
                        }
                    }
                }
                
                c++;
            }

            if (popupWindow)
            {
                if (Event.current.type == EventType.MouseDown ||
                    EditorWindow.focusedWindow != SmartHierarchy.active.editorWindow &&
                    EditorWindow.focusedWindow != popupWindow)
                    popupWindow.Close();
            }
            
            void OpenPopup(Rect buttonRect, Rect toolbarRect, Component component)
            {
                focusedItem = viewItem;

                // Don't let popup get in the way of toolbar area.
                buttonRect.xMax = toolbarRect.xMax;
                
                if (!popupWindow)
                    popupWindow = ComponentPopupWindow.CreateAndShow(component, buttonRect, popupSize);
                else
                    popupWindow.ShowWith(component, buttonRect, popupSize);
            }
        }
    }
}