using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

namespace AV.Hierarchy
{
    public abstract class PopupElement : VisualElement
    {
        public class TabToggle : ToolbarToggle
        {
            public TabToggle(Texture icon)
            {
                var iconGUI = default(IMGUIContainer);
                iconGUI = new IMGUIContainer(() =>
                {
                    GUI.color = new Color(1, 1, 1, resolvedStyle.opacity);

                    var rect = RectUtils.GetCenteredRect(new Rect(0, 0, 16, 16), iconGUI.layout);

                    ViewItemGUI.DrawFlatIcon(rect, icon, GUIColors.FlatIcon, isOn: value);
                });
                
                iconGUI.AddToClassList("tab-toggle-icon");
                
                Add(iconGUI);
            }
        }
        
        private class BlurZone : VisualElement {}

        private static UIResources Resource => UIResources.Index;
        
        private static Texture2D InfoIcon = IconContent("console.infoicon.sml").image as Texture2D;
        private static Texture2D HelpIcon = IconContent("_Help").image as Texture2D;
        
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
            
            var borderColor = new Color(0, 0, 0, 1);
            
            background.StretchToParentSize();
            background.style.borderTopColor = borderColor * new Color(1, 1, 1, 0.085f);
            background.style.borderLeftColor = borderColor * new Color(1, 1, 1, 0.125f);
            background.style.borderRightColor = borderColor * new Color(1, 1, 1, 0.125f);
            background.style.borderBottomColor = borderColor * new Color(1, 1, 1, 0.14f);
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
                var helpButton = new VisualElement { style = { backgroundImage = HelpIcon }};
                
                helpButton.AddToClassList("title-button");
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
        
        public static T GetPopup<T>() where T : PopupElement
        {
            activePopups.TryGetValue(typeof(T), out var popup);
            return (T)popup;
        }
        
        public static VisualElement CreateSpace(float height)
        {
            return new VisualElement { style = { height = height } };
        }
        
        public static VisualElement CreateSeparator()
        {
            var separator = new VisualElement();
            separator.AddToClassList("separator");
            return separator;
        }
        
        public static VisualElement CreateImage(Texture2D texture)
        {
            return new VisualElement { style =
            {
                backgroundImage = texture, 
                minWidth = 16, minHeight = 16 
            }};
        }

        public static VisualElement CreateLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("label");
            return label;
        }
        
        public static VisualElement CreateHelpBox(string text)
        {
            var helpBox = new VisualElement();
            helpBox.styleSheets.Add(UIResources.Index.helpBoxStyle);
            helpBox.AddToClassList("help-box");
            helpBox.AddToClassList("help-box-mini");
            
            var infoIcon = CreateImage(InfoIcon);
            infoIcon.name = "Icon";
            
            var label = CreateLabel(text);
            label.name = "Message";
            
            helpBox.Add(infoIcon);
            helpBox.Add(label);
            
            return helpBox;
        }
        
        public static Foldout CreateFoldout(string text)
        {
            var foldout = new Foldout { text = text };
            
            foldout.Query(className: "unity-toggle").First().style.marginLeft = 0;
            foldout.contentContainer.style.marginLeft = 0;

            return foldout;
        }

        public static Toolbar CreateTabsToolbar()
        {
            var toolbar = new Toolbar();
            toolbar.AddToClassList("tabs-toolbar");
            return toolbar;
        }
        
        public static TabToggle CreateTabToggle(GUIContent content, SerializedProperty property)
        {
            var toggle = new TabToggle(content.image);

            // Bug in 2020.3.1~5 - checkmark is visible on ToolbarToggle
            var checkmark = toggle.Query(className: "unity-toggle__input").First();
            checkmark?.RemoveFromHierarchy();

            toggle.BindProperty(property);
            toggle.AddToClassList("tab-toggle");

            return toggle;
        }

        public static VisualElement CreateActiveToggleMarker()
        {
            var marker = new VisualElement { name = "ActiveToggleMarker" };
            marker.AddToClassList("active-toggle-marker");
            return marker;
        }
    }
}