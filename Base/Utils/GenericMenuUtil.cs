using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class GenericMenuUtil
    {
        private static readonly Type menuItemType = typeof(GenericMenu).GetNestedType("MenuItem", BindingFlags.NonPublic | BindingFlags.Instance);

#if UNITY_2021_1_OR_NEWER
        private static readonly FieldInfo menuItemsField = typeof(GenericMenu).GetField("m_MenuItems", BindingFlags.NonPublic | BindingFlags.Instance);
#else
        private static readonly FieldInfo menuItemsField = typeof(GenericMenu).GetField("menuItems", BindingFlags.NonPublic | BindingFlags.Instance);
#endif

        private static readonly FieldInfo menuItemContent = menuItemType.GetField("content", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo menuItemFunc = menuItemType.GetField("func", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo menuItemFunc2 = menuItemType.GetField("func2", BindingFlags.Public | BindingFlags.Instance);

#if UNITY_2021_1_OR_NEWER
        public static IList GetItems(GenericMenu menu)
        {
            return (IList)menuItemsField.GetValue(menu);
        }
#else
        public static ArrayList GetItems(GenericMenu menu)
        {
            return (ArrayList)menuItemsField.GetValue(menu);
        }
#endif

        public static GUIContent GetContent(object menuItem)
        {
            return (GUIContent)menuItemContent.GetValue(menuItem);
        }

        public static GenericMenu.MenuFunction GetFunc(object menuItem)
        {
            return (GenericMenu.MenuFunction)menuItemFunc.GetValue(menuItem);
        }

        public static GenericMenu.MenuFunction2 GetFuncWithUserData(object menuItem)
        {
            return (GenericMenu.MenuFunction2)menuItemFunc2.GetValue(menuItem);
        }

        public static void SetFunc(object menuItem, GenericMenu.MenuFunction func)
        {
            menuItemFunc.SetValue(menuItem, func);
        }
    }
}
