
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class SceneHierarchyPatch : PatchBase
    {
        private static GUIStyle sceneHeaderBg;
        
        protected override IEnumerable<Patch> GetPatches()
        {
            var type = EditorAssembly.GetType("UnityEditor.SceneHierarchy");
            
            var onGUI = AccessTools.Method(type, "OnGUI");

            yield return new Patch(onGUI, prefix: nameof(_OnGUI), postfix: nameof(OnGUI_));
        }

        private static void _OnGUI(EditorWindow ___m_EditorWindow, Rect rect)
        {
            if (sceneHeaderBg == null)
            {
                sceneHeaderBg = "SceneTopBarBg";
                sceneHeaderBg.fixedHeight = 0;
            }

            HierarchyInitialization.GerOrCreateForWindow(___m_EditorWindow, out var hierarchy);
            HierarchyInitialization.ActiveWindow = ___m_EditorWindow;
            
            hierarchy.OnBeforeGUI();
        }
        
        private static void OnGUI_(EditorWindow ___m_EditorWindow, Rect rect)
        {
            HierarchyInitialization.GerOrCreateForWindow(___m_EditorWindow, out var hierarchy);
            
            hierarchy.OnAfterGUI();
        }
    }
}