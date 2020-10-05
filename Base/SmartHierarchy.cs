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
        private static Texture2D folderIcon;
        private static Texture2D folderEmptyIcon;
        private static GUIStyle iconStyle;

        private static class Reflected
        {
            // We need to get SceneHierarchy TreeView to change items icon
            private static readonly PropertyInfo getLastInteractedHierarchyWindow;
            private static readonly PropertyInfo getSceneHierarchy;
            private static readonly FieldInfo getTreeView;
            private static readonly Func<object> GetHierarchyWindow;

            // Takes hierarchy TreeViewController and instance ID
            public static readonly Func<object, int, TreeViewItem> FindHierarchyItem;

            // Caches hierarchies tree views
            private static readonly Dictionary<object, object> HierarchyTreeViews = new Dictionary<object, object>();

            static Reflected()
            {
                var sceneHierarchyWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
                var sceneHierarchyType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchy");
                var treeViewControllerType =
                    typeof(TreeViewState).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");

                // As all required types are internal, we need to do some reflection
                // See https://github.com/Unity-Technologies/UnityCsReference/blob/2020.1/Editor/Mono/SceneHierarchyWindow.cs

                getLastInteractedHierarchyWindow = sceneHierarchyWindowType
                    .GetProperty("lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.Static);
                getSceneHierarchy = sceneHierarchyWindowType.GetProperty("sceneHierarchy");
                getTreeView = sceneHierarchyType.GetField("m_TreeView", BindingFlags.NonPublic | BindingFlags.Instance);

                var findItemMethod = treeViewControllerType.GetMethod("FindItem");

                // Compile delegate from reflection so there is no performance drop from invoking
                GetHierarchyWindow = Expression
                    .Lambda<Func<object>>(Expression.Property(null, getLastInteractedHierarchyWindow)).Compile();

                var objParam = Expression.Parameter(typeof(object));
                var intParam = Expression.Parameter(typeof(int));
                var objToTreeViewConvert = Expression.Convert(objParam, treeViewControllerType);

                FindHierarchyItem = Expression.Lambda<Func<object, int, TreeViewItem>>(
                        Expression.Call(objToTreeViewConvert, findItemMethod, intParam), objParam, intParam)
                    .Compile();
            }

            public static object GetHierarchyTreeView()
            {
                var hierarchyWindow = GetHierarchyWindow();

                if (HierarchyTreeViews.TryGetValue(hierarchyWindow, out var treeView))
                    return treeView;

                var sceneHierarchy = getSceneHierarchy.GetValue(hierarchyWindow);
                treeView = getTreeView.GetValue(sceneHierarchy);

                return treeView;
            }
        }

        private class ItemData
        {
            public bool isFolder;
            public Texture2D icon;
            public bool hasIcon;
            public int initialDepth;
        }
        private class FolderData
        {
            public bool isEmpty;
            public GameObject mainGameObject;
        }

        private static readonly Dictionary<GameObject, ItemData> ItemsData = new Dictionary<GameObject, ItemData>();
        private static readonly Dictionary<GameObject, FolderData> FoldersData = new Dictionary<GameObject, FolderData>();

        static SmartHierarchy()
        {
            EditorApplication.hierarchyChanged += () =>
            {
                ItemsData.Clear();
                FoldersData.Clear();
            };
            
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
            folderIcon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D;
            folderEmptyIcon = EditorGUIUtility.IconContent("FolderEmpty Icon").image as Texture2D;
        }

        private static TreeViewItem FindItem(int id)
        {
            return Reflected.FindHierarchyItem(Reflected.GetHierarchyTreeView(), id);
        }

        private static void OnHierarchyItemGUI(int instanceId, Rect rect)
        {
            if (iconStyle == null)
            {
                iconStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(0, 0, 0, 0)
                };
            }
            
            var gameObject = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            
            if (gameObject == null)
                return;
            
            var view = FindItem(instanceId);
            
            if (!ItemsData.TryGetValue(gameObject, out var item))
            {
                item = new ItemData()
                {
                    isFolder = gameObject.TryGetComponent<Folder>(out _),
                    initialDepth = view.depth
                };
                var components = gameObject.GetComponents<Component>();
                
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
                
                ItemsData.Add(gameObject, item);
            }
            
            if (item.isFolder)
            {
                if (!FoldersData.TryGetValue(gameObject, out var data))
                {
                    data = new FolderData()
                    {
                        isEmpty = gameObject.transform.childCount == 0
                    };
                    
                    if (!data.isEmpty)
                    {
                        data.mainGameObject = gameObject.transform.GetChild(0).gameObject;
                    }
                    
                    FoldersData.Add(gameObject, data);
                }
                
                view.icon = data.isEmpty ? folderEmptyIcon : folderIcon;

                var isHover = rect.Contains(Event.current.mousePosition);
                var toggleRect = new Rect(rect) {x = rect.xMax - 16, width = 16};
                var isActive = gameObject.activeSelf;

                if (isHover || !isActive)
                {
                    EditorGUI.BeginChangeCheck();
                    GUI.Toggle(toggleRect, isActive, GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                        gameObject.SetActive(!isActive);
                }
            }
            else
            {
                if (item.hasIcon)
                    view.icon = item.icon;
                
                if (Application.isPlaying)
                    view.depth = item.initialDepth + 1;
                
                // TODO: Show GO components on hover
                //if (rect.Contains(Event.current.mousePosition))
                //{
                //    var iconRect = new Rect(rect) {x = rect.xMax - 16};
                //    GUI.Label(iconRect, mainItem.icon, iconStyle);
                //}
            }
        }
    }
}
#endif
