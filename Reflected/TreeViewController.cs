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

        public static Func<object, TreeViewItem> hoveredItemFunc;
        public static Func<object, int, int> getRowFunc;

        public static Func<object, int, TreeViewItem> getItemFunc;
        public static Func<object, int, bool> isExpandedFunc;

        private static PropertyInfo dataProperty;
        private static FieldInfo onVisibleRowsChangedField;

        public TreeViewController(object controller)
        {
            data = dataProperty.GetValue(controller);
            this.controller = controller;
        }

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            var controllerType =
                typeof(TreeViewState).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");

            dataProperty = controllerType.GetProperty("data");

            var goTreeViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectTreeViewDataSource");

            onVisibleRowsChangedField =
                goTreeViewType.GetField("onVisibleRowsChanged");

            var hoveredItem = controllerType.GetProperty("hoveredItem");
            var getRowMethod = goTreeViewType.GetMethod("GetRow");
            var getItemMethod = goTreeViewType.GetMethod("GetItem");
            var isExpandedMethod = goTreeViewType.GetMethod("IsExpanded", new[] {typeof(int)});

            var objParam = Parameter(typeof(object));
            var intParam = Parameter(typeof(int));
            var controllerConvert = Convert(objParam, controllerType);
            var goTreeViewConvert = Convert(objParam, goTreeViewType);

            hoveredItemFunc = Lambda<Func<object, TreeViewItem>>(Property(controllerConvert, hoveredItem), objParam).Compile();
            
            getRowFunc = Lambda<Func<object, int, int>>(
                Call(goTreeViewConvert, getRowMethod, intParam), objParam, intParam).Compile();

            getItemFunc = Lambda<Func<object, int, TreeViewItem>>(
                Call(goTreeViewConvert, getItemMethod, intParam), objParam, intParam).Compile();

            isExpandedFunc = Lambda<Func<object, int, bool>>(
                Call(goTreeViewConvert, isExpandedMethod, intParam), objParam, intParam).Compile();
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
        
        public void SetOnVisibleRowsChanged(Action action)
        {
            var onVisibleRowsChanged = onVisibleRowsChangedField.GetValue(data) as Action;
            onVisibleRowsChanged += action;
            onVisibleRowsChangedField.SetValue(data, onVisibleRowsChanged);
        }
    }
}