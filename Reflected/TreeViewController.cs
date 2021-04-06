using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using static System.Linq.Expressions.Expression;

namespace AV.Hierarchy
{
    internal class TreeViewController
    {
        private object data; // GameObjectTreeViewDataSource
        public object instance; // TreeViewController

        public TreeViewGUI gui;

        public static Func<object, TreeViewItem> hoveredItemFunc;
        private static Func<object, TreeViewItem, bool> isItemSelected;
        private static Func<object, int, int> getRowFunc;
        private static Func<object, bool> hasFocus;

        private static Func<object, int, TreeViewItem> getItemFunc;
        private static Func<object, int, bool> isExpandedFunc;
        private static Func<object, TreeViewItem, bool> isExpandableFunc;
        private static Action<object, TreeViewItem, int, bool> setUserExpandedFunc;
        private static Action<object, TreeViewItem, bool> selectionClickFunc;
        private static Func<object, object> getGUI;

        private static PropertyInfo dataProperty;
        private static FieldInfo onVisibleRowsChangedField;

        public TreeViewController(object instance)
        {
            data = dataProperty.GetValue(instance);
            gui = new TreeViewGUI(GetTreeViewGUI(instance));
            this.instance = instance;
        }

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            var controllerType = typeof(TreeViewState).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");

            dataProperty = controllerType.GetProperty("data");
            var guiProperty = controllerType.GetProperty("gui");

            var goTreeDataType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectTreeViewDataSource");

            onVisibleRowsChangedField = goTreeDataType.GetField("onVisibleRowsChanged");

            var hoveredItem = controllerType.GetProperty("hoveredItem");
            var isSelectedMethod = controllerType.GetMethod("IsItemDragSelectedOrSelected");
            var hasFocusMethod = controllerType.GetMethod("HasFocus");
            
            var getRowMethod = goTreeDataType.GetMethod("GetRow");
            var getItemMethod = goTreeDataType.GetMethod("GetItem");
            var isExpandedMethod = goTreeDataType.GetMethod("IsExpanded", new[] { typeof(int) });
            var isExpandableMethod = goTreeDataType.GetMethod("IsExpandable", new[] { typeof(TreeViewItem) });
            var setUserExpandedMethod = controllerType.GetMethod("UserInputChangedExpandedState");
            var selectionClickMethod = controllerType.GetMethod("SelectionClick");

            var objParam = Parameter(typeof(object));
            var intParam = Parameter(typeof(int));
            var boolParam = Parameter(typeof(bool));
            var itemParam = Parameter(typeof(TreeViewItem));
            var controllerConvert = Convert(objParam, controllerType);
            var goTreeDataConvert = Convert(objParam, goTreeDataType);

            getGUI = Lambda<Func<object, object>>(Call(controllerConvert, guiProperty.GetMethod), objParam).Compile();
            
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
            
            isExpandableFunc = Lambda<Func<object, TreeViewItem, bool>>(
                Call(goTreeDataConvert, isExpandableMethod, itemParam), objParam, itemParam).Compile();
            
            selectionClickFunc = Lambda<Action<object, TreeViewItem, bool>>(
                Call(controllerConvert, selectionClickMethod, itemParam, boolParam), objParam, itemParam, boolParam).Compile();
            
            setUserExpandedFunc = Lambda<Action<object, TreeViewItem, int, bool>>(
                Call(controllerConvert, setUserExpandedMethod, itemParam, intParam, boolParam), objParam, itemParam, intParam, boolParam).Compile();
        }

        public static object GetTreeViewGUI(object controller)
        {
            return getGUI.Invoke(controller);
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
        
        public bool IsExpandable(TreeViewItem item)
        {
            return isExpandableFunc.Invoke(data, item);
        }

        public void SetUserExpanded(TreeViewItem item, int row, bool expanded)
        {
            setUserExpandedFunc.Invoke(instance, item, row, expanded);
        }

        public bool IsSelected(TreeViewItem item)
        {
            return isItemSelected.Invoke(instance, item);
        }

        public void SelectionClick(TreeViewItem itemClicked, bool keepMultiSelection)
        {
            selectionClickFunc.Invoke(instance, itemClicked, keepMultiSelection);
        }

        public bool HasFocus()
        {
            return hasFocus(instance);
        }
        
        public void SetOnVisibleRowsChanged(Action action)
        {
            var onVisibleRowsChanged = onVisibleRowsChangedField.GetValue(data) as Action;
            onVisibleRowsChanged += action;
            onVisibleRowsChangedField.SetValue(data, onVisibleRowsChanged);
        }
    }
}