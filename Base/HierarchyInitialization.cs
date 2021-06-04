using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static System.Linq.Expressions.Expression;

namespace AV.Hierarchy
{
    internal static class HierarchyInitialization
    {
        public static EditorWindow ActiveWindow;
        
        private static PropertyInfo lastInteractedHierarchyWindow;
        private static Func<object> getLastHierarchyWindowFunc;
        
        private static readonly Dictionary<EditorWindow, SmartHierarchy> Hierarchies = new Dictionary<EditorWindow, SmartHierarchy>();
        
        
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            var sceneHierarchyWindowType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            
            lastInteractedHierarchyWindow = sceneHierarchyWindowType
                .GetProperty("lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.Static);
            
            getLastHierarchyWindowFunc = Lambda<Func<object>>(Property(null, lastInteractedHierarchyWindow)).Compile();
        }
        
        internal static void GerOrCreateForWindow(EditorWindow window, out SmartHierarchy hierarchy)
        {
            if (!Hierarchies.TryGetValue(window, out hierarchy))
            {
                hierarchy = new SmartHierarchy(window);
                Hierarchies.Add(window, hierarchy);
            }
        }

        internal static SmartHierarchy GetActiveHierarchy()
        {
            if (!ActiveWindow)
                return null;
            Hierarchies.TryGetValue(ActiveWindow, out var hierarchy);
            return hierarchy;
        }
        
        internal static SmartHierarchy GetLastInteractedHierarchy()
        {
            var lastHierarchyWindow = getLastHierarchyWindowFunc() as EditorWindow;

            if (lastHierarchyWindow == null)
                return null;
            
            return Hierarchies[lastHierarchyWindow];
        }
    }
}