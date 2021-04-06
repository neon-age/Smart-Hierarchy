using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    [InitializeOnLoad]
    internal static class ObjectIconUtil
    {
        private static Func<Object, Texture2D> getIconForObject;
        
        static ObjectIconUtil()
        {
            var getIconForObjectInfo = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);

            var objParam = Expression.Parameter(typeof(Object));
            getIconForObject = Expression.Lambda<Func<Object, Texture2D>>(Expression.Call(getIconForObjectInfo, objParam), objParam).Compile();
        }
        
        public static Texture2D GetObjectGizmoIcon(Object obj)
        {
            return getIconForObject.Invoke(obj);
        }
    }
}
