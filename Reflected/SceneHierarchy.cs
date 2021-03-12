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
        internal TreeViewState state;
        internal TreeViewController controller;
        
        public Action onExpandedStateChange;
        public Action onVisibleRowsChanged;
        public Action onTreeViewReload;

        public TreeViewItem hoveredItem => TreeViewController.hoveredItemFunc(controller.controller);

        private static MethodInfo pasteGO;
        private static MethodInfo duplicateGO;
        
        private static FieldInfo controllerField;
        private static FieldInfo stateField;

        public SceneHierarchy(object hierarchy)
        {
            this.hierarchy = hierarchy;
            controller = new TreeViewController(controllerField.GetValue(hierarchy));
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

            controllerField = sceneHierarchyType.GetField("m_TreeView", BindingFlags.NonPublic | BindingFlags.Instance);
            stateField = sceneHierarchyType.GetField("m_TreeViewState", BindingFlags.NonPublic | BindingFlags.Instance);
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
            var actualController = controllerField.GetValue(hierarchy);

            // Was controller been re-initialized?
            if (actualController != controller.controller)
            {
                controller = new TreeViewController(actualController);
                
                controller.SetOnVisibleRowsChanged(onVisibleRowsChanged);
                onTreeViewReload?.Invoke();
            }
            
            state = stateField.GetValue(hierarchy) as TreeViewState;
        }
    }
}