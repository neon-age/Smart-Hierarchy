using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class ComponentPopupWindow : EditorPopupWindow
    {
        public Component target => component;
        
        [SerializeField] Component component;
        [SerializeField] Vector2 newSize;
        [SerializeField] bool isPinned; // to be implemented
        
        VisualElement root => rootVisualElement;
        ComponentPopup popup;
        bool layoutChanged;
        
        
        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                Resources.FindObjectsOfTypeAll<ComponentPopupWindow>()
                    .Where(x => x.isPinned).ToList()
                    .ForEach(x => x.Close());
            };
        }
        
        public static ComponentPopupWindow CreateAndShow(Component component, Rect buttonRect, Vector2 size)
        {
            size.y += 2; // Border size

            var window = CreateInstance<ComponentPopupWindow>();
            
            window.component = component;
            window.GetLocalSize(buttonRect, size);
            
            buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
            window.CreateAt(buttonRect, size);
            
            return window;
        }

        private void GetLocalSize(Rect buttonRect, Vector2 size)
        {
            var localRect = new Rect(buttonRect.position, size);
            localRect.y += buttonRect.height;
            
            newSize = localRect.size;
        }

        public void ShowWith(Component component, Rect buttonRect, Vector2 size)
        {
            size.y += 2;
            
            this.component = component;
            GetLocalSize(buttonRect, size);
            
            Initialize();
            EditorUtility.SetDirty(this);
            Repaint();
            EditorApplication.update.Invoke();
            
            buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
            ShowAt(buttonRect, size);
        }

        private void Initialize()
        {
            if (target == null)
                return;
        
            root.Clear();
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
            // Closing window during Update throws null exceptions.
            popup.onDestroy = Close;
        }
        
        public void OnBecameVisible()
        {
            Initialize();
        }
        
        public void OnDisable()
        {
            popup?.Dispose();
        }

        // Window must be resized during update, or otherwise we'll get flickering.
        private void Update()
        {
            if (target == null)
            {
                try
                {
                    Close();
                }
                catch
                {
                    DestroyImmediate(this);
                    return;
                }
            }

            if (layoutChanged)
            {
                layoutChanged = false;
                
                minSize = newSize;
                maxSize = newSize;
                
                this.RepaintImmediately();
            }
        }

        private void ApplyBackgroundStyle()
        {
            popup.style.SetBorderWidth(1);
            popup.style.SetBorderRadius(0);
            popup.style.SetBorderColor(popup.style.backgroundColor.value);
           
            popup.style.backgroundColor = (Color)(isProSkin ? new Color32(60, 60, 60, 255) : new Color32(200, 200, 200, 255));
        }
    }
}