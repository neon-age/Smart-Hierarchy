using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

namespace AV.Hierarchy
{
    public abstract class ObjectPopupWindow : VisualElement
    {
        private static StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath("d7461833a510d124191fbed727ac19f0"));
        private static Texture2D arrowIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("70cf301939ec64147b3b646ec72c2cf2"));
        
        private static int controlHash = nameof(ObjectPopupWindow).GetHashCode();
        
        public static ObjectPopupWindow current;
        
        public VisualElement titleContainer { get; }
        public override VisualElement contentContainer { get; }

        public TextElement title { get; }
        private VisualElement contextArrow;

        private Vector2 position;
        private VisualElement root;

        protected virtual Color backgroundColor => isProSkin ? new Color32(40, 40, 40, 230) : new Color32(165, 165, 165, 230);
        
        protected ObjectPopupWindow()
        {
            styleSheets.Add(styleSheet);
            style.backgroundColor = backgroundColor;
            
            AddToClassList("popup-window");
            
            contentContainer = new VisualElement();
            
            contentContainer.AddToClassList("content-container");
            
            titleContainer = new VisualElement();
            titleContainer.AddToClassList("title-container");
            
            title = new TextElement { name = "Title" };
            titleContainer.Add(title);
            
            hierarchy.Add(titleContainer);
            hierarchy.Add(CreateSeparator());
            hierarchy.Add(contentContainer);
        }

        public VisualElement CreateSeparator()
        {
            var separator = new VisualElement();
            separator.AddToClassList("separator");
            return separator;
        }

        public virtual void OnLoseFocus()
        {
            Close();
        }

        public static void LoseFocus() => current?.OnLoseFocus();

        public static void Close()
        {
            current?.RemoveFromHierarchy();
            current = null;
        }

        public void ShowInsideWindow(Vector2 position, VisualElement root)
        {
            Close();

            current = this;
            
            position.x -= 7;
            position.y -= 5;
            
            this.root = root;
            this.position = position;
            
            root.Add(this);
            
            RegisterCallback<MouseDownEvent>(evt => evt.StopImmediatePropagation());
            RegisterCallback<ContextClickEvent>(evt => evt.StopImmediatePropagation());
            
            GUIUtility.keyboardControl = controlHash;
            
            root.RegisterCallback<GeometryChangedEvent>(OnRootGeometryChange);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
            
            style.top = position.y;
            
            contextArrow = new VisualElement { style = { backgroundImage = arrowIcon, position = Position.Absolute } };
            contextArrow.style.unityBackgroundImageTintColor = backgroundColor;
            contextArrow.AddToClassList("context-arrow");
            
            hierarchy.Insert(0, contextArrow);
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
            var rootWidth = root.layout.width - 14;

            if (xMax > rootWidth)
                style.left = layout.x + (rootWidth - layout.xMax);
            else
                style.left = position.x;
        }
    }
}