using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class ObjectIconUtil
    {
        private static MethodInfo getIconForObject = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
        
        public static Texture2D GetIconForObject(Object obj)
        {
            return (Texture2D)getIconForObject.Invoke(null, new []{ obj });
        }
    }
}
