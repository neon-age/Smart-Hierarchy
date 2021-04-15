using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class SceneHierarchyPatch : HarmonyPatchProvider<SceneHierarchyPatch>
    {
        [InitializeOnLoadMethod]
        static void OnLoad() => Initialize();
        
        protected override void OnInitialize()
        {
            var type = EditorAssembly.GetType("UnityEditor.SceneHierarchy");
            
            var onGUI = type.GetMethod("OnGUI", BindingFlags.Public | BindingFlags.Instance);

            Patch(onGUI, nameof(OnGUI_Prefix), nameof(OnGUI_Postfix));
        }

        private static void OnGUI_Prefix(EditorWindow ___m_EditorWindow, Rect rect)
        {
            HierarchyInitialization.GerOrCreateForWindow(___m_EditorWindow, out var hierarchy);
            HierarchyInitialization.ActiveWindow = ___m_EditorWindow;
            
            hierarchy.OnBeforeGUI();
        }
        
        private static void OnGUI_Postfix(EditorWindow ___m_EditorWindow, Rect rect)
        {
            HierarchyInitialization.GerOrCreateForWindow(___m_EditorWindow, out var hierarchy);
            
            hierarchy.OnAfterGUI();
        }
    }
}
