using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static System.Linq.Expressions.Expression;

namespace AV.Hierarchy
{
    public static class HierarchyInitialization
    {
        private static PropertyInfo lastInteractedHierarchyWindow;

        private static Func<object> getLastHierarchyWindowFunc;
        
        private static Dictionary<object, SmartHierarchy> Hierarchies = new Dictionary<object, SmartHierarchy>();
        
        
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            DoReflection();
        }
        
        private static void DoReflection()
        {
            var sceneHierarchyWindowType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            
            lastInteractedHierarchyWindow = sceneHierarchyWindowType
                .GetProperty("lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.Static);
            
            getLastHierarchyWindowFunc = Lambda<Func<object>>(Property(null, lastInteractedHierarchyWindow)).Compile();
        }
        
        internal static SmartHierarchy GetLastHierarchy()
        {
            var lastHierarchyWindow = getLastHierarchyWindowFunc();

            if (!Hierarchies.TryGetValue(lastHierarchyWindow, out var hierarchy))
            {
                hierarchy = new SmartHierarchy(lastHierarchyWindow as EditorWindow);
                Hierarchies.Add(lastHierarchyWindow, hierarchy);
            }
            return hierarchy;
        }
    }
}