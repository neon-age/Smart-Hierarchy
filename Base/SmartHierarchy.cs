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

        internal static SmartHierarchy active { get; private set; }
        internal static bool isPluginEnabled => prefs.enableSmartHierarchy;

        private static bool isGUIPatched;

        internal SceneHierarchyWindow window { get; }
        internal SceneHierarchy hierarchy => window.hierarchy;
        
        internal TreeViewGUI gui => controller.gui;
        internal TreeViewState state => hierarchy.state;
        internal TreeViewController controller => hierarchy.controller;
        
        private EditorWindow actualWindow => window.actualWindow;
        private bool isHovering => hoveredItem != null;
        private int hoveredItemId => hierarchy.hoveredItem?.id ?? -1;
        public TreeViewItem hoveredItem => hierarchy.hoveredItem;
        
        private bool isInitialized;
        private Vector2 localMousePosition;
        
        public readonly VisualElement root;
        private readonly HoverPreview hoverPreview;
        private readonly IMGUIContainer guiContainer;
        private readonly TreeViewGUIArgs treeViewArgs = new TreeViewGUIArgs();
        private readonly Dictionary<int, HierarchyItemBase> itemsData = new Dictionary<int, HierarchyItemBase>();
        
        public static readonly Dictionary<object, SmartHierarchy> Instances = new Dictionary<object, SmartHierarchy>();
        
        
        public SmartHierarchy(EditorWindow window)
        {
            root = window.rootVisualElement;
            this.window = new SceneHierarchyWindow(window);
            
            hoverPreview = new HoverPreview();

            RegisterCallbacks();
            hierarchy.ReassignCallbacks();
            
            guiContainer = root.parent.Query<IMGUIContainer>().First();
            var guiHandler = guiContainer.onGUIHandler;
            
            guiContainer.onGUIHandler = OnBeforeGUI;
            guiContainer.onGUIHandler += guiHandler;
            guiContainer.onGUIHandler += OnAfterGUI;
            
            root.Add(hoverPreview);
            
            hierarchy.EnsureValidData();
        }
        
        private static void OnGUIPatch()
        {
            TreeViewGUIPatch.Initialize();
            GameObjectTreeViewGUIPatch.Initialize();
        }

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            EditorApplication.update += GetLastActiveHierarchy;
            
            //EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        private static void GetLastActiveHierarchy()
        {
            active = HierarchyInitialization.GetLastHierarchy();
        }

        private void RegisterCallbacks()
        {
            HierarchySettingsProvider.onChange += OnSettingsChange;

            Selection.selectionChanged += ReloadView;
            hierarchy.onVisibleRowsChanged += ReloadView;
            EditorApplication.hierarchyChanged += ReloadView;
            EditorApplication.playModeStateChanged += change => isInitialized = false;
        }

        private void OnSettingsChange()
        {
            isInitialized = false;
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
        
        private void OnEnable()
        {
            if (!Instances.ContainsKey(gui.instance))
                Instances.Add(gui.instance, this);

            hierarchy.EnsureValidData();
            actualWindow.SetAntiAliasing(8);
            actualWindow.minSize = new Vector2(4, 64);
        }

        private void OnDisable()
        {
            if (Instances.ContainsKey(gui.instance))
                Instances.Remove(gui.instance);
        }
        
        private void OnBeforeGUI()
        {
            if (!prefs.enableSmartHierarchy)
            {
                isInitialized = false;
                OnDisable();
                return;
            }
            
            if (!isGUIPatched)
            {
                isGUIPatched = true;
                OnGUIPatch();
            }
            
            if (!isInitialized)
            {
                isInitialized = true;
                OnEnable();
            }
            
            active = HierarchyInitialization.GetLastHierarchy();

            hierarchy.EnsureValidData();
            treeViewArgs.baseIndent = gui.GetBaseIndent();
            treeViewArgs.controller = controller;
            
            CopyPasteCommands.ExecuteCommands();
        }

        internal void RemoveItem(int id)
        {
            itemsData.Remove(id);
        }
        
        public bool TryGetOrCreateItem(TreeViewItem treeItem, out HierarchyItemBase item)
        {
            var id = treeItem.id;
            
            if (!itemsData.TryGetValue(id, out item))
            {
                var instance = EditorUtility.InstanceIDToObject(id);
                
                item = HierarchyItemCreator.CreateForInstance(id, instance, treeItem);
                item.SetTreeViewData(treeItem, treeViewArgs);
                item.OnAfterCreation();

                itemsData.Add(id, item);
            }

            return item != null;
        }

        private void OnAfterGUI()
        {
            if (!isPluginEnabled)
                return;

            // Mouse is relative to window during onGUIHandler
            if (evt.type != EventType.Used)
            {
                localMousePosition = evt.mousePosition;
                
                hoverPreview.SetPosition(localMousePosition, actualWindow.position);
            }

            HandleObjectPreview();
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
                itemsData.TryGetValue(hoveredItemId, out var item);
                hoverPreview.OnItemPreview(item);
            }
            else
            {
                hoverPreview.Hide();
            }
        }
    }
}