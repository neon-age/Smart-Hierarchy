using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using static System.Linq.Expressions.Expression;

namespace AV.Hierarchy
{
    internal class SceneHierarchy
    {
        private object hierarchy;
        internal TreeViewState state;
        internal TreeViewController controller;
        
        public Action onExpandedStateChange;
        public Action onVisibleRowsChanged;
        public Action onTreeViewReload;

        public TreeViewItem hoveredItem => TreeViewController.hoveredItemFunc(controller.instance);

        private static MethodInfo pasteGO;
        private static MethodInfo duplicateGO;
        
        private static Func<object, object> getController;
        private static Func<object, TreeViewState> getTreeViewState;

        public SceneHierarchy(object hierarchy)
        {
            this.hierarchy = hierarchy;
            controller = new TreeViewController(getController.Invoke(hierarchy));
        }

        public void ReassignCallbacks()
        {
            controller.SetOnVisibleRowsChanged(onVisibleRowsChanged);
        }
        
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            var sceneHierarchyType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchy");

            pasteGO = sceneHierarchyType.GetMethod("PasteGO", BindingFlags.NonPublic | BindingFlags.Instance);
            duplicateGO = sceneHierarchyType.GetMethod("DuplicateGO", BindingFlags.NonPublic | BindingFlags.Instance);

            var controllerProperty = sceneHierarchyType.GetProperty("treeView", BindingFlags.NonPublic | BindingFlags.Instance);

            var objParam = Parameter(typeof(object));
            var convert = Convert(objParam, sceneHierarchyType);
            getController = Lambda<Func<object, object>>(Property(convert, controllerProperty), objParam).Compile();
            
            var stateField = sceneHierarchyType.GetField("m_TreeViewState", BindingFlags.NonPublic | BindingFlags.Instance);
            
            getTreeViewState = Lambda<Func<object, TreeViewState>>(Field(convert, stateField), objParam).Compile();
        }

        public void PasteGO()
        {
            pasteGO.Invoke(hierarchy, null);
        }

        public void DuplicateGO()
        {
            duplicateGO.Invoke(hierarchy, null);
        }

        public TreeViewItem GetViewItem(int id)
        {
            // GetRow checks every rows for required id.
            // It's much faster then recursive FindItem, but still needs to be called only when TreeView is changed.
            var row = controller.GetRow(id);
            if (row == -1)
                return null;

            return controller.GetItem(row);
        }

        public void EnsureValidData()
        {
            state = getTreeViewState.Invoke(hierarchy);
            
            var actualController = getController.Invoke(hierarchy);

            if (actualController != controller.instance)
            {
                controller = new TreeViewController(actualController);
                
                controller.SetOnVisibleRowsChanged(onVisibleRowsChanged);
                onTreeViewReload?.Invoke();
            }
        }
    }
}