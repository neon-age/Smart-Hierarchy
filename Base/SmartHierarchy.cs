#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Object = UnityEngine.Object;

namespace AV.Editor.Hierarchy
{
    [InitializeOnLoad]
    internal static class SmartHierarchy
    {
        //private static HierarchySettings.Preferences preferences;
        private static Texture2D folderIcon;
        private static Texture2D folderEmptyIcon;
        private static GUIStyle iconStyle;

        private class ItemData
        {
            internal GameObject instance;
            internal TreeViewItem view;
            
            internal bool isFolder;
            internal Texture2D icon;
            internal bool hasIcon;
            
            internal Texture2D initialIcon;
            internal int initialDepth;
        }
        private class FolderData
        {
            internal bool isEmpty;
            internal GameObject mainGameObject;
        }

        private static readonly Dictionary<int, ItemData> ItemsData = new Dictionary<int, ItemData>();
        private static readonly Dictionary<int, FolderData> FoldersData = new Dictionary<int, FolderData>();

        static SmartHierarchy()
        {
            //var settingsProvider = HierarchySettings.GetProvider();
            //preferences = settingsProvider.preferences;
            
            //settingsProvider.onChange += Reinitialize;
            EditorApplication.hierarchyChanged += Reinitialize;
            Reflected.onExpandedStateChange = ClearViewData;
            
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
            
            folderIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
            folderEmptyIcon = EditorGUIUtility.IconContent("FolderEmpty Icon").image as Texture2D;
        }

        private static void ClearViewData()
        {
            foreach (var item in ItemsData.Values)
            {
                item.view = null;
            }
        }
        
        private static void Reinitialize()
        {
            ItemsData.Clear();
            FoldersData.Clear();
        }

        private static void OnHierarchyItemGUI(int instanceId, Rect rect)
        {
            //if (!preferences.enableSmartHierarchy)
            //    return;
            
            if (iconStyle == null)
            {
                iconStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(0, 0, 0, 0)
                };
            }

            GameObject instance;
            
            if (!ItemsData.TryGetValue(instanceId, out var item))
            {
                instance = EditorUtility.InstanceIDToObject(instanceId) as GameObject;

                if (instance == null)
                    return;
                
                //Debug.Log("item " + instanceId);
                item = new ItemData
                {
                    instance = instance,
                    isFolder = instance.TryGetComponent<Folder>(out _)
                };
                var components = instance.GetComponents<Component>();
                
                if (components.Length > 1)
                {
                    var firstComponent = components[1];
                    if (firstComponent)
                    {
                        // Usually, main component is at the top, but for UI it's the opposite
                        var isUI = components.Length > 1 && components[1] is CanvasRenderer;
                        var component = components[isUI ? components.Length - 1 : components.Length > 1 ? 1 : 0];
                        
                        if(component)
                            item.icon = EditorGUIUtility.ObjectContent(component, component.GetType()).image as Texture2D;
                        item.hasIcon = item.icon != null;
                    }
                }
                
                ItemsData.Add(instanceId, item);
            }

            if (item == null)
                return;

            var fullWidthRect = GetFullWidthRect(rect);
            instance = item.instance;

            if (item.view == null || item.view.id != instanceId)
            {
                item.view = Reflected.FindItem(instanceId);

                // Happens to be null when entering prefab mode
                if (item.view == null)
                    return;
                
                item.initialDepth = item.view.depth;
            }

            if (item.isFolder)
            {
                if (!FoldersData.TryGetValue(instanceId, out var data))
                {
                    data = new FolderData
                    {
                        isEmpty = instance.transform.childCount == 0
                    };
                    
                    if (!data.isEmpty)
                    {
                        data.mainGameObject = instance.transform.GetChild(0).gameObject;
                    }
                    
                    FoldersData.Add(instanceId, data);
                }
                
                item.view.icon = data.isEmpty ? folderEmptyIcon : folderIcon;
            }
            else
            {
                //if (preferences.displayMainComponentIcon)
                //{
                    if (item.hasIcon)
                        item.view.icon = item.icon;
                //}

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

            // Caches hierarchies tree views
            private static readonly Dictionary<object, object> HierarchyTreeViews = new Dictionary<object, object>();

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

                if (!HierarchyTreeViews.TryGetValue(hierarchyWindow, out var treeView))
                {
                    var sceneHierarchy = getSceneHierarchy.GetValue(hierarchyWindow);
                    treeView = getTreeView.GetValue(sceneHierarchy);

                    // For god sake, I forgot to add this line previously..
                    HierarchyTreeViews.Add(hierarchyWindow, treeView);

                    SetOnExpandedStateChanged(treeView, onExpandedStateChange);
                    return treeView;
                }

                return treeView;
            }
            
            public static void SetOnExpandedStateChanged(object treeView, Action onExpandedStateChanged)
            {
                var action = (Action)expandedStateChanged.GetValue(treeView);
                
                Debug.Log(action);
                action += onExpandedStateChanged;
                expandedStateChanged.SetValue(treeView, action);
            }
        }
    }
}
#endif
