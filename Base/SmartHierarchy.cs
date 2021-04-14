using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AV.Hierarchy
{
    internal class SmartHierarchy
    {
        internal static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;
        internal static HierarchyOptions options => HierarchyOptions.instance;
        internal static Event evt => Event.current;
        internal static SmartHierarchy active { get; private set; }

        internal SceneHierarchyWindow window { get; }
        internal SceneHierarchy hierarchy => window.hierarchy;
        internal TreeViewState state => hierarchy.state;
        internal TreeViewController controller => hierarchy.controller;
        internal float time => Time.realtimeSinceStartup;
        internal float baseIndent => controller.gui.GetBaseIndent();
        
        private EditorWindow actualWindow => window.actualWindow;
        private ViewItem hoveredItem;
        private bool isHovering => hoveredItem != null;
        private int hoveredItemId => hierarchy.hoveredItem?.id ?? -1;
        private bool isInitialized = true;
        private bool requiresGUISetup = true;
        private Vector2 localMousePosition;
        
        public readonly VisualElement root;
        private readonly HoverPreview hoverPreview;
        private readonly IMGUIContainer guiContainer;
        private readonly Dictionary<int, ViewItem> ItemsData = new Dictionary<int, ViewItem>();
        
        public SmartHierarchy(EditorWindow window)
        {
            root = window.rootVisualElement;
            this.window = new SceneHierarchyWindow(window);
            
            hoverPreview = new HoverPreview();

            Initialize();
            RegisterCallbacks();
            hierarchy.ReassignCallbacks();
            
            guiContainer = root.parent.Query<IMGUIContainer>().First();
            
            // onGUIHandler is called after hierarchy GUI, thus has a slight delay
            guiContainer.onGUIHandler += OnAfterGUI;
            var guiAction = guiContainer.onGUIHandler;

            guiContainer.onGUIHandler = OnBeforeGUI;
            guiContainer.onGUIHandler += guiAction;
            guiContainer.onGUIHandler += OnAfterGUI;

            root.Add(hoverPreview);
        }

        private void Initialize()
        {
            requiresGUISetup = true;
        }

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        internal void OnDisable()
        {
            isInitialized = false;
            controller.gui.SetBaseIndent(32);
        }

        private void RegisterCallbacks()
        {
            HierarchySettingsProvider.onChange += OnSettingsChange;

            Selection.selectionChanged += ReloadView;
            hierarchy.onVisibleRowsChanged += ReloadView;
            EditorApplication.hierarchyChanged += ReloadView;
            EditorApplication.playModeStateChanged += change => Initialize();
        }

        private void OnSettingsChange()
        {
            Initialize();
            ReloadView();
            ImmediateRepaint();
        }
        
        public void ReloadView()
        {
            ItemsData.Clear();
        }

        private static void ImmediateRepaint()
        {
            EditorApplication.RepaintHierarchyWindow();
        }
        
        private void OnBeforeGUI()
        {
            if (!prefs.enableSmartHierarchy)
            {
                if (isInitialized)
                    OnDisable();
                return;
            }

            isInitialized = true;

            if (requiresGUISetup)
            {
                requiresGUISetup = false;
                OnGUISetup();
            }
            hierarchy.EnsureValidData();

            ItemsData.TryGetValue(hoveredItemId, out hoveredItem);

            CopyPasteCommands.ExecuteCommands();
            SetupBaseIndent();
            HideDefaultIcon();
        }

        private static void OnHierarchyItemGUI(int id, Rect rect)
        {
            if (!prefs.enableSmartHierarchy)
                return;
            
            active = HierarchyInitialization.GetLastHierarchy();

            active.OnItemCallback(id, rect);
        }

        private void OnItemCallback(int id, Rect rect)
        {
            OnItemGUI(id, rect);
        }

        private void OnGUISetup()
        {
            //hierarchy.EnsureValidData();
            actualWindow.SetAntiAliasing(8);
            SceneVisGUIPatch.Initialize();
        }

        private void SetupBaseIndent()
        {
            var visibility = options.showVisibilityToggle;
            var picking = options.showPickingToggle;
            
            var indent = !visibility && !picking ? 6 : 0;

            if (options.showVisibilityToggle)
                indent += 16;
            
            if (options.showPickingToggle)
                indent += 16;
            
            controller.gui.SetBaseIndent(indent);
        }
        
        private void OnItemGUI(int id, Rect rect)
        {
            var instance = EditorUtility.InstanceIDToObject(id) as GameObject;

            if (!instance)
                return;
                
            GetInstanceViewItem(id, instance, rect, out var item);
            
            // Happens to be null when entering prefab mode
            if (!item.EnsureViewExist(hierarchy))
                return;
            
            HideDefaultIcon();
            
            var isSelected = controller.IsSelected(item.view);
            var isHover = hierarchy.hoveredItem == item.view;
            var isOn = isSelected && controller.HasFocus();
            
            item.DoItemGUI(this, rect, isHover, isOn);
        }
        
        private void OnAfterGUI()
        {
            if (!prefs.enableSmartHierarchy)
                return;
            
            // Makes sure other items like scene headers are not interrupted 
            controller.gui.ResetCustomStyling();
            
            // Mouse is relative to window during onGUIHandler
            if (evt.type != EventType.Used)
            {
                localMousePosition = evt.mousePosition;
                
                hoverPreview.SetPosition(localMousePosition, actualWindow.position);
            }

            HandleObjectPreview();
        }

        private void HideDefaultIcon()
        {
            // Changing icon in TreeViewItem is not enough,
            // When item is selected, it is hardcoded to use "On" icon (white version for blue background).
            // https://github.com/Unity-Technologies/UnityCsReference/blob/2019.4/Editor/Mono/GUI/TreeView/TreeViewGUI.cs#L157
            
            // Setting width to zero will hide default icon, so we can draw our own on top,
            // But this also removes item text indentation and "Pinging" icon..
            controller.gui.SetIconWidth(0);
            
            controller.gui.SetSpaceBetweenIconAndText(18);
        }
        
        private void HandleObjectPreview()
        {
            var isPreviewKeyHold = false;
        
            switch (prefs.previewKey)
            {
                case ModificationKey.Alt: isPreviewKeyHold = evt.alt; break;
                case ModificationKey.Shift: isPreviewKeyHold = evt.shift; break;
                case ModificationKey.Control: isPreviewKeyHold = evt.control; break;
            }
        
            if (isHovering && prefs.enableHoverPreview && isPreviewKeyHold)
            {
                hoverPreview.OnItemPreview(hoveredItem);
            }
            else
            {
                hoverPreview.Hide();
            }
        }
        
        private void GetInstanceViewItem(int id, GameObject instance, Rect rect, out ViewItem item)
        {
            if (!ItemsData.TryGetValue(id, out item))
            {
                item = new ViewItem(instance) { rect = rect };

                ItemsData.Add(id, item);
            }
        }
    }
}