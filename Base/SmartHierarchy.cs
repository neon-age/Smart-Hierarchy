using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal class SmartHierarchy
    {
        internal static Event evt => Event.current;
        internal static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;

        internal static bool isPluginEnabled { get; private set; } = prefs.enableSmartHierarchy;
        internal static SmartHierarchy active { get; private set; }

        internal SceneHierarchyWindow window { get; }
        internal SceneHierarchy hierarchy => window.hierarchy;
        internal TreeViewState state => hierarchy.state;
        internal TreeViewController controller => hierarchy.controller;
        
        private EditorWindow actualWindow => window.actualWindow;
        private bool isHovering => hoveredItem != null;
        private int hoveredItemId => hierarchy.hoveredItem?.id ?? -1;
        
        private HierarchyItem hoveredItem;
        
        private bool requiresGUISetup = true;
        private Vector2 localMousePosition;
        
        public readonly VisualElement root;
        private readonly HoverPreview hoverPreview;
        private readonly IMGUIContainer guiContainer;
        private readonly Dictionary<int, HierarchyItem> itemsData = new Dictionary<int, HierarchyItem>();
        
        public SmartHierarchy(EditorWindow window)
        {
            root = window.rootVisualElement;
            this.window = new SceneHierarchyWindow(window);
            
            hoverPreview = new HoverPreview();

            Initialize();
            RegisterCallbacks();
            hierarchy.ReassignCallbacks();
            
            guiContainer = root.parent.Query<IMGUIContainer>().First();
            var guiHandler = guiContainer.onGUIHandler;
            
            guiContainer.onGUIHandler = OnBeforeGUI;
            guiContainer.onGUIHandler += guiHandler;
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
            itemsData.Clear();
        }
        
        private static void ImmediateRepaint()
        {
            EditorApplication.DirtyHierarchyWindowSorting();
        }
        
        private static void OnHierarchyItemGUI(int id, Rect rect)
        {
            if (!isPluginEnabled)
                return;
            
            active = HierarchyInitialization.GetLastHierarchy();

            active.OnItemCallback(id, rect);
        }

        private void OnItemCallback(int id, Rect rect)
        {
            if (requiresGUISetup)
            {
                requiresGUISetup = false;
                OnGUISetup();
            }

            OnItemGUI(id, rect);
        }

        private void OnGUISetup()
        {
            hierarchy.EnsureValidData();
            actualWindow.SetAntiAliasing(8);
        }

        private void OnDisable()
        {
            controller.gui.ResetCustomStyling();
        }
        
        
        private void OnBeforeGUI()
        {
            if (!prefs.enableSmartHierarchy)
            {
                isPluginEnabled = false;
                OnDisable();
                return;
            }
            isPluginEnabled = true;

            hierarchy.EnsureValidData();
            HideDefaultIcon();

            itemsData.TryGetValue(hoveredItemId, out hoveredItem);

            CopyPasteCommands.ExecuteCommands();
            HideDefaultIcon();
        }
        
        private void OnItemGUI(int id, Rect rect)
        {
            var instance = EditorUtility.InstanceIDToObject(id);

            GetInstanceViewItem(id, instance, out var item);
            
            // Happens to be null when entering prefab mode
            if (!item.EnsureViewExist(hierarchy))
                return;
            
            HideDefaultIcon();
            
            var isSelected = controller.IsSelected(item.view);
            var isHover = hierarchy.hoveredItem == item.view;
            var isOn = isSelected && controller.HasFocus();
            
            item.DoItemGUI(new HierarchyItemArgs { rect = rect, focused = isSelected, hovered = isHover, on = isOn });
        }
        
        private void OnAfterGUI()
        {
            if (!isPluginEnabled)
                return;

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
        
        private void GetInstanceViewItem(int id, Object instance, out HierarchyItem item)
        {
            if (!itemsData.TryGetValue(id, out item))
            {
                item = HierarchyItemCreator.CreateForInstance(id, instance);

                itemsData.Add(id, item);
            }
        }
    }
}