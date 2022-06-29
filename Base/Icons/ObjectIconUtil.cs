using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class ObjectIconUtil
    {
#if UNITY_2021_2_OR_NEWER
        private static MethodInfo getIconForObject = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.Public | BindingFlags.Static);
#else
        private static MethodInfo getIconForObject = typeof(EditorGUIUtility).GetMethod("GetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
#endif
        
        public static Texture2D GetIconForObject(Object obj)
        {
            return (Texture2D)getIconForObject.Invoke(null, new []{ obj });
        }
    }
}
