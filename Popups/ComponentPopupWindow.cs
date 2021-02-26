using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    public class ComponentPopupWindow : EditorWindow
    {
        public Component target { get; private set; }
        public Action onClose;

        private VisualElement root => rootVisualElement;
        
        private ComponentPopup popup;
        private Vector2 newSize;
        private bool layoutChanged;
        
        private static MethodInfo repaintImmediately = typeof(EditorWindow).GetMethod("RepaintImmediately", BindingFlags.NonPublic | BindingFlags.Instance);

        public static ComponentPopupWindow Show(Component component, Rect buttonRect, Vector2 size)
        {
            size.y += 2; // Border size

            var localRect = new Rect(buttonRect.position, size);
            localRect.y += buttonRect.height;
            
            var popup = CreateInstance<ComponentPopupWindow>();
            popup.Create(component, localRect);

            buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
            popup.ShowAsDropDown(buttonRect, size);
            
            GUI.UnfocusWindow();
            
            return popup;
        }
        
        private void Create(Component component, Rect rect)
        {
            target = component;
            newSize = rect.size;
        }
        
        public void OnBecameVisible()
        {
            this.SetAntiAliasing(8);
            
            popup = new ComponentPopup(target);
            root.Add(popup);
            
            popup.style.overflow = Overflow.Hidden;
            popup.style.position = Position.Absolute;
            popup.style.left = 0;
            popup.style.right = 0;

            ApplyBackgroundStyle();
            
            popup.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                newSize = evt.newRect.size;
                layoutChanged = true;
            });

            repaintImmediately.Invoke(this, null);
        }
        
        public void OnDisable()
        {
            popup?.Dispose();
        }

        private void OnBecameInvisible()
        {
            onClose?.Invoke();
        }

        // Window must be resized during update, or otherwise we'll get flickering.
        private void Update()
        {
            if (target == null)
                Close();
            
            if (layoutChanged)
            {
                layoutChanged = false;
                
                minSize = newSize;
                maxSize = newSize;
                
                repaintImmediately.Invoke(this, null);

                // TODO (not important): Find a way to fix black frame that may appear when clicking on enum popup during any expansion animation.
            }
        }

        private void ApplyBackgroundStyle()
        {
            popup.style.SetBorderRadius(0);
            popup.style.SetBorderColor(popup.style.backgroundColor.value);
            popup.style.SetBorderWidth(1);
            popup.style.backgroundColor = (Color)(isProSkin ? new Color32(60, 60, 60, 255) : new Color32(200, 200, 200, 255));
        }
    }
}