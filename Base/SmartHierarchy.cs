#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    [InitializeOnLoad]
    internal static class SmartHierarchy
    {
        private static HierarchyPreferences preferences;
        private static Texture2D folderIcon;
        private static Texture2D folderEmptyIcon;
        private static GUIStyle iconStyle;

        private class ItemData
        {
            internal GameObject instance;
            
            internal TreeViewItem view;
            internal Texture2D icon;
            internal int initialDepth;
            internal int lastViewId;
            
            internal bool isPrefab;
            internal bool isRootPrefab;
            internal bool isFolder;
        }
        private class FolderData
        {
            internal bool isEmpty;
            internal GameObject mainGameObject;
        }

        private static readonly Dictionary<GameObject, ItemData> ItemsData = new Dictionary<GameObject, ItemData>();
        private static readonly Dictionary<GameObject, FolderData> FoldersData = new Dictionary<GameObject, FolderData>();

        static SmartHierarchy()
        {
            var settingsProvider = HierarchySettingsProvider.GetProvider();
            preferences = settingsProvider.preferences;
            
            settingsProvider.onChange += Reinitialize;
            //EditorApplication.hierarchyChanged += Reinitialize;
            Reflected.onExpandedStateChange = ClearViewData;
            
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
            
            folderIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
            folderEmptyIcon = EditorGUIUtility.IconContent("FolderEmpty Icon").image as Texture2D;
        }

        private static void ClearViewData()
        {
            //foreach (var item in ItemsData.Values)
            //{
            //    item.view = null;
            //}
        }
        
        private static void Reinitialize()
        {
            ItemsData.Clear();
            FoldersData.Clear();
        }

        private static void OnHierarchyItemGUI(int instanceId, Rect rect)
        {
            if (!preferences.enableSmartHierarchy)
                return;
            
            if (iconStyle == null)
            {
                iconStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(0, 0, 0, 0)
                };
            }

            var instance = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            
            if (instance == null)
                return;
            
            if (!ItemsData.TryGetValue(instance, out var item))
            {
                if (instance == null)
                    return;
                
                item = new ItemData
                {
                    instance = instance,
                    isPrefab = PrefabUtility.GetPrefabAssetType(instance) == PrefabAssetType.Regular,
                    isRootPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(instance),
                    isFolder = instance.TryGetComponent<Folder>(out _)
                };
                var components = instance.GetComponents<Component>();

                var mainComponent = DecideMainComponent(components);
                if (mainComponent != null)
                    item.icon = EditorGUIUtility.ObjectContent(mainComponent, mainComponent.GetType()).image as Texture2D;
                
                ItemsData.Add(instance, item);
            }

            var fullWidthRect = GetFullWidthRect(rect);
            instance = item.instance;
            
            if (item.lastViewId != instanceId)
            {
                item.view = Reflected.FindItem(instanceId);
                item.lastViewId = item.view.id;
                item.initialDepth = item.view.depth;
            }
            
            // Happens to be null when entering prefab mode
            if (item.view == null)
            {
                ItemsData.Remove(instance);
                return;
            }

            if (item.isFolder)
            {
                if (!FoldersData.TryGetValue(instance, out var folder))
                {
                    folder = new FolderData
                    {
                        isEmpty = instance.transform.childCount == 0
                    };
                    
                    if (!folder.isEmpty)
                    {
                        folder.mainGameObject = instance.transform.GetChild(0).gameObject;
                    }
                    
                    FoldersData.Add(instance, folder);
                }
                
                item.view.icon = folder.isEmpty ? folderEmptyIcon : folderIcon;
            }
            else
            {
                if (item.icon != null)
                {
                    switch (preferences.stickyComponentIcon)
                    {
                        case StickyIcon.Never: break;
                        case StickyIcon.OnAnyObject:
                            item.view.icon = item.icon;
                            break;
                        case StickyIcon.NotOnPrefabs:
                            if (!item.isRootPrefab)
                                item.view.icon = item.icon;
                            break;
                    }
                }

                if (Application.isPlaying)
                    item.view.depth = item.initialDepth + 1;
                
                // TODO: Show GO components on hover
                //if (rect.Contains(Event.current.mousePosition))
                //{
                //    var iconRect = new Rect(rect) {x = rect.xMax - 16};
                //    GUI.Label(iconRect, mainItem.icon, iconStyle);
                //}
            }
            
            if (IsHoveringItem(fullWidthRect))
            {
                var toggleRect = new Rect(fullWidthRect) { x = 32 };
                if (OnLeftToggle(toggleRect, instance.activeSelf, out var isActive))
                {
                    instance.SetActive(isActive);
                }
            }
        }

        private static Component DecideMainComponent(params Component[] components)
        {
            var count = components.Length;
            if (count == 0) 
                return null;
            
            var zeroComponent = components[0];
            
            if (count == 1)
            {
                switch (preferences.transformIcon)
                {
                    case TransformIcon.Always: 
                        return zeroComponent;
                    
                    case TransformIcon.OnUniqueOrigin:
                        if (zeroComponent is Transform transform)
                        {
                            if (transform.localPosition != Vector3.zero || 
                                transform.localRotation != Quaternion.identity)
                                return zeroComponent;
                        }
                        break;
                    
                    case TransformIcon.OnlyRectTransform:
                        return zeroComponent is RectTransform ? zeroComponent : null;
                }

                return null;
            }
            
            if (HasCanvasRenderer(components))
            {
                return GetMainUGUIComponent(components);
            }
            
            return components[1];
        }

        private static bool HasCanvasRenderer(params Component[] components)
        {
            return components.OfType<CanvasRenderer>().Any();
        }

        private static Component GetMainUGUIComponent(params Component[] components)
        {
            Graphic lastGraphic = null;
            Selectable lastSelectable = null;

            foreach (var component in components)
            {
                if (component is Graphic graphic)
                    lastGraphic = graphic;

                if (component is Selectable selectable)
                    lastSelectable = selectable;
            }

            if (lastSelectable != null)
                return lastSelectable;

            if (lastGraphic != null)
                return lastGraphic;

            return null;
        }

        private static Rect GetFullWidthRect(Rect rect)
        {
            var fullWidthRect = new Rect(rect);
            fullWidthRect.x = 0;
            fullWidthRect.width = Screen.width;
            return fullWidthRect;
        }

        private static bool IsHoveringItem(Rect rect)
        {
            return rect.Contains(Event.current.mousePosition);
        }

        private static bool OnLeftToggle(Rect rect, bool isActive, out bool value)
        {
            var toggleRect = new Rect(rect) { width = 16 };
            
            EditorGUI.BeginChangeCheck();
            value = GUI.Toggle(toggleRect, isActive, GUIContent.none);
            return EditorGUI.EndChangeCheck();
        }
        
        private static class Reflected
        {
            public static Action onExpandedStateChange;
            
            // We need to get SceneHierarchy TreeView to change items icon
            private static readonly PropertyInfo getLastInteractedHierarchyWindow;
            private static readonly PropertyInfo getSceneHierarchy;
            private static readonly PropertyInfo expandedStateChanged;
            private static readonly FieldInfo getTreeView;
            
            private static readonly Func<object> GetLastHierarchyWindow;

            // Takes hierarchy TreeViewController and instance ID
            private static readonly Func<object, int, TreeViewItem> FindHierarchyItem;
            
            static Reflected()
            {
                var sceneHierarchyWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
                var sceneHierarchyType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchy");
                var treeViewControllerType =
                    typeof(TreeViewState).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");

                expandedStateChanged = treeViewControllerType.GetProperty("expandedStateChanged");
                
                // As all required types are internal, we need to do some reflection
                // See https://github.com/Unity-Technologies/UnityCsReference/blob/2020.1/Editor/Mono/SceneHierarchyWindow.cs

                getLastInteractedHierarchyWindow = sceneHierarchyWindowType
                    .GetProperty("lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.Static);
                getSceneHierarchy = sceneHierarchyWindowType.GetProperty("sceneHierarchy");
                getTreeView = sceneHierarchyType.GetField("m_TreeView", BindingFlags.NonPublic | BindingFlags.Instance);

                var findItemMethod = treeViewControllerType.GetMethod("FindItem");

                // Compile delegate from reflection so there is no performance drop from invoking
                GetLastHierarchyWindow = Expression
                    .Lambda<Func<object>>(Expression.Property(null, getLastInteractedHierarchyWindow)).Compile();

                var objParam = Expression.Parameter(typeof(object));
                var intParam = Expression.Parameter(typeof(int));
                var objToTreeViewConvert = Expression.Convert(objParam, treeViewControllerType);

                FindHierarchyItem = Expression.Lambda<Func<object, int, TreeViewItem>>(
                        Expression.Call(objToTreeViewConvert, findItemMethod, intParam), objParam, intParam)
                    .Compile();
            }

            // Calling FindItem frequently on large hierarchies is very slow. Do this only when TreeView is being changed. 
            public static TreeViewItem FindItem(int id)
            {
                return FindHierarchyItem(GetLastHierarchyTreeView(), id);
            }
            
            public static object GetLastHierarchyTreeView()
            {
                var hierarchyWindow = GetLastHierarchyWindow();

                // TreeView is rebuilding when entering/exiting Prefab Mode, so we can't simply cache it
                // Reflection performance is not so bad, comparing to FindItem.. 
                var sceneHierarchy = getSceneHierarchy.GetValue(hierarchyWindow);
                var treeView = getTreeView.GetValue(sceneHierarchy);

                SetOnExpandedStateChanged(treeView, onExpandedStateChange);
                
                return treeView;
            }
            
            public static void SetOnExpandedStateChanged(object treeView, Action onExpandedStateChanged)
            {
                var action = (Action)expandedStateChanged.GetValue(treeView);
                
                //Debug.Log(action);
                action += onExpandedStateChanged;
                expandedStateChanged.SetValue(treeView, action);
            }
        }
    }
}
#endif
