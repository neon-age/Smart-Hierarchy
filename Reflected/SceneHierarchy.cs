using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using static System.Linq.Expressions.Expression;

namespace AV.Hierarchy
{
    internal class SceneHierarchy
    {
        private object hierarchy;
        internal TreeViewController controller;
        
        public Action onExpandedStateChange;
        public Action onVisibleRowsChanged;
        public Action onTreeViewReload;

        private static FieldInfo treeViewControllerField;

        public SceneHierarchy(object hierarchy)
        {
            this.hierarchy = hierarchy;
            controller = new TreeViewController(treeViewControllerField.GetValue(hierarchy));
        }

        public void ReassignCallbacks()
        {
            controller.SetOnVisibleRowsChanged(onVisibleRowsChanged);
        }
        
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            var sceneHierarchyType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchy");

            treeViewControllerField =
                sceneHierarchyType.GetField("m_TreeView", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public TreeViewItem GetViewItem(int id)
        {
            var controller = GetTreeViewController();

            // GetRow checks every rows for required id.
            // It's much faster then recursive FindItem, but still needs to be called only when TreeView is changed.
            var row = controller.GetRow(id);
            if (row == -1)
                return null;

            return controller.GetItem(row);
        }

        public TreeViewController GetTreeViewController()
        {
            // Reflection performance is not so bad comparing to FindItem.. 
            var treeViewController = treeViewControllerField.GetValue(hierarchy);

            // Has controller been re-initialized? (for ex. during entering/exiting Prefab Mode)
            if (treeViewController != controller.controller)
            {
                controller = new TreeViewController(treeViewController);
                
                controller.SetOnVisibleRowsChanged(onVisibleRowsChanged);
                onTreeViewReload?.Invoke();
            }

            return controller;
        }
    }
}