using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

namespace AV.Hierarchy
{
    public abstract class PopupElement : VisualElement
    {
        private class BlurZone : VisualElement {}

        private static UIResources Resource => UIResources.Index;
        
        private static Texture2D helpIcon = IconContent("_Help").image as Texture2D;
        
        private static PopupElement active;
        private static Dictionary<Type, PopupElement> activePopups = new Dictionary<Type, PopupElement>();

        public string title
        {
            get => titleText.text;
            set => titleText.text = value;
        }
        public VisualElement titleContainer { get; }
        public override VisualElement contentContainer { get; }

        public override bool canGrabFocus => true;

        protected virtual string helpURL { get; }

        private TextElement titleText;
        private VisualElement contextArrow;
        private VisualElement blurZone;

        private Vector2 position;
        protected VisualElement root;
        protected VisualElement boxShadow;
        protected EditorWindow window;

        private Color backgroundColor => isProSkin ? new Color32(50, 50, 50, 250) : new Color32(195, 195, 195, 250);
        
        
        protected PopupElement()
        {
            styleSheets.Add(Resource.popupElementStyle);
            style.backgroundColor = new Color(0, 0, 0, 0.01f);
            AddToClassList("popup-window");

            var background = new VisualElement { style = { backgroundColor = backgroundColor }};
            
            const int ShadowSize = 6;
            boxShadow = new VisualElement { style =
            {
                position = Position.Absolute,
                left = -ShadowSize, right = -ShadowSize, 
                top = -ShadowSize, bottom = -ShadowSize,
                opacity = 0.1f, 
                backgroundImage = Resource.boxShadow
            }};
            boxShadow.style.SetSlice(12);
            //hierarchy.Add(boxShadow);
            hierarchy.Add(background);
            
            var borderColor = isProSkin ? new Color(0, 0, 0, 1) : new Color(0, 0, 0, 1);
            
            background.StretchToParentSize();
            background.style.borderTopColor = borderColor * new Color32(255, 255, 255, 22);
            background.style.borderLeftColor = borderColor * new Color32(255, 255, 255, 32);
            background.style.borderRightColor = borderColor * new Color32(255, 255, 255, 32);
            background.style.borderBottomColor = borderColor * new Color32(255, 255, 255, 36);
            background.style.SetBorderRadius(8);
            background.style.SetBorderWidth(1);
            
            contextArrow = new VisualElement { style = 
            {
                backgroundImage = Resource.contextArrow, 
                position = Position.Absolute
            }};
            contextArrow.style.unityBackgroundImageTintColor = backgroundColor;
            contextArrow.AddToClassList("context-arrow");
            
            hierarchy.Add(contextArrow);
            
            
            contentContainer = new VisualElement();
            
            contentContainer.AddToClassList("content-container");
            
            titleContainer = new VisualElement();
            titleContainer.AddToClassList("title-container");
            
            titleText = new TextElement { name = "Title" };
            titleContainer.Add(titleText);
            
            hierarchy.Add(titleContainer);

            if (!string.IsNullOrEmpty(helpURL))
            {
                var helpButton = new VisualElement { style = { opacity = 0.9f, backgroundImage = helpIcon, width = 16, height = 16 }};
                helpButton.tooltip = "Open Documentation";
                helpButton.RegisterCallback<MouseUpEvent>(evt => Application.OpenURL(helpURL));

                titleContainer.Add(helpButton);
            }

            hierarchy.Add(contentContainer);

            var type = GetType();
            
            if (activePopups.TryGetValue(GetType(), out var activePopup))
                activePopup.Close();

            activePopups.Add(type, this);
        }

        private void AttachToPanel(AttachToPanelEvent evt)
        {
            OnAttach(evt);
            UnregisterCallback<AttachToPanelEvent>(AttachToPanel);
        }

        protected virtual void OnAttach(AttachToPanelEvent evt)
        {
        }
        
        public static T GetPopup<T>() where T : PopupElement
        {
            activePopups.TryGetValue(typeof(T), out var popup);
            return (T)popup;
        }

        public static VisualElement CreateSeparator()
        {
            var separator = new VisualElement();
            separator.AddToClassList("separator");
            return separator;
        }

        public void Close()
        {
            EditorApplication.update -= OnUpdate;
            
            RemoveFromHierarchy();
            blurZone?.RemoveFromHierarchy();
            
            activePopups.Remove(GetType());
        }

        private void OnUpdate()
        {
            //if (EditorWindow.focusedWindow != window)
            //    Close();
        }
        
        public void ShowInsideWindow(Vector2 position, EditorWindow window, VisualElement rootVisualElement = null)
        {
            active = this;
            EditorApplication.update += OnUpdate;
            
            position.x -= 7;
            position.y -= 5;

            this.root = rootVisualElement ?? window.rootVisualElement;
            this.window = window;
            this.position = position;

            CreateBlurZoneInRoot();
            
            RegisterCallback<AttachToPanelEvent>(AttachToPanel);
            
            root.Add(this);

            var typeName = GetType().Name;
            GUI.SetNextControlName(typeName);
            GUI.FocusControl(typeName);
            Focus();
            
            root.RegisterCallback<GeometryChangedEvent>(OnRootGeometryChange);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChange);

            style.top = position.y;
        }

        private void CreateBlurZoneInRoot()
        {
            blurZone = new BlurZone { name = "PopupBlurZone" };
            blurZone.RegisterCallback<MouseDownEvent>(evt => Close());
            
            root.Add(blurZone);
            blurZone.StretchToParentSize();
        }

        private void OnRootGeometryChange(GeometryChangedEvent evt)
        {
            FitToRootWidth();
        }

        private void OnGeometryChange(GeometryChangedEvent evt)
        {
            FitToRootWidth();
            
            contextArrow.style.left = position.x - layout.x + resolvedStyle.paddingLeft;
        }

        private void FitToRootWidth()
        {
            var xMax = position.x + layout.width;
            var rootWidth = root.layout.width;

            if (xMax > rootWidth)
                style.left = layout.x + (rootWidth - layout.xMax);
            else
                style.left = position.x;
        }
    }
}