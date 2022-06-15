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
      
        private static readonly MemberInfo menuItemsField = typeof(GenericMenu).GetMember("menuItems", BindingFlags.NonPublic | BindingFlags.Instance)[0];
        private static readonly FieldInfo menuItemContent = menuItemType.GetField("content", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo menuItemFunc = menuItemType.GetField("func", BindingFlags.Public | BindingFlags.Instance);
        private static readonly FieldInfo menuItemFunc2 = menuItemType.GetField("func2", BindingFlags.Public | BindingFlags.Instance);

        public static IList GetItems(GenericMenu menu)
        {
            if (menuItemsField is FieldInfo field) return (IList)field.GetValue(menu);
            if (menuItemsField is PropertyInfo prop) return (IList)prop.GetValue(menu); return null;
        }

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
