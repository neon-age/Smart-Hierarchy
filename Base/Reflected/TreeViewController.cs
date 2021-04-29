using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using static System.Linq.Expressions.Expression;

namespace AV.Hierarchy
{
    internal class TreeViewController
    {
        private object data; // GameObjectTreeViewDataSource
        public object controller; // TreeViewController

        public TreeViewGUI gui;

        public static Func<object, TreeViewItem> hoveredItemFunc;
        private static Func<object, TreeViewItem, bool> isItemSelected;
        private static Func<object, int, int> getRowFunc;
        private static Func<object, bool> hasFocus;

        private static Func<object, int, TreeViewItem> getItemFunc;
        private static Func<object, int, bool> isExpandedFunc;

        private static PropertyInfo dataProperty;
        private static PropertyInfo guiProperty;
        private static FieldInfo onVisibleRowsChangedField;

        public TreeViewController(object controller)
        {
            data = dataProperty.GetValue(controller);
            gui = new TreeViewGUI(guiProperty.GetValue(controller));
            this.controller = controller;
        }

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            var controllerType =
                typeof(TreeViewState).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");

            dataProperty = controllerType.GetProperty("data");
            guiProperty = controllerType.GetProperty("gui");

            var goTreeDataType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectTreeViewDataSource");

            onVisibleRowsChangedField = goTreeDataType.GetField("onVisibleRowsChanged");

            var hoveredItem = controllerType.GetProperty("hoveredItem");
            var isSelectedMethod = controllerType.GetMethod("IsItemDragSelectedOrSelected");
            var hasFocusMethod = controllerType.GetMethod("HasFocus");
            
            var getRowMethod = goTreeDataType.GetMethod("GetRow");
            var getItemMethod = goTreeDataType.GetMethod("GetItem");
            var isExpandedMethod = goTreeDataType.GetMethod("IsExpanded", new[] { typeof(int) });
            

            var objParam = Parameter(typeof(object));
            var intParam = Parameter(typeof(int));
            var itemParam = Parameter(typeof(TreeViewItem));
            var controllerConvert = Convert(objParam, controllerType);
            var goTreeDataConvert = Convert(objParam, goTreeDataType);

            hasFocus = Lambda<Func<object, bool>>(
                Call(controllerConvert, hasFocusMethod), objParam).Compile();
            
            hoveredItemFunc = Lambda<Func<object, TreeViewItem>>(
                Property(controllerConvert, hoveredItem), objParam).Compile();
            
            isItemSelected = Lambda<Func<object, TreeViewItem, bool>>(
                Call(controllerConvert, isSelectedMethod, itemParam), objParam, itemParam).Compile();
            
            getRowFunc = Lambda<Func<object, int, int>>(
                Call(goTreeDataConvert, getRowMethod, intParam), objParam, intParam).Compile();

            getItemFunc = Lambda<Func<object, int, TreeViewItem>>(
                Call(goTreeDataConvert, getItemMethod, intParam), objParam, intParam).Compile();

            isExpandedFunc = Lambda<Func<object, int, bool>>(
                Call(goTreeDataConvert, isExpandedMethod, intParam), objParam, intParam).Compile();
        }

        public int GetRow(int id)
        {
            return getRowFunc(data, id);
        }
        
        public TreeViewItem GetItem(int row)
        {
            // There's an error during undo
            try
            {
                return getItemFunc(data, row);
            }
            catch
            {
                return null;
            }
        }

        public bool IsExpanded(int instanceID)
        {
            return isExpandedFunc.Invoke(data, instanceID);
        }

        public bool IsSelected(TreeViewItem item)
        {
            return isItemSelected.Invoke(controller, item);
        }

        public bool HasFocus()
        {
            return hasFocus(controller);
        }
        
        public void SetOnVisibleRowsChanged(Action action)
        {
            var onVisibleRowsChanged = onVisibleRowsChangedField.GetValue(data) as Action;
            onVisibleRowsChanged += action;
            onVisibleRowsChangedField.SetValue(data, onVisibleRowsChanged);
        }
    }
}