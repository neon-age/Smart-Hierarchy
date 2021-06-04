
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class SceneVisGUIPatch : PatchBase
    {
        private static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;
        
        private static bool pluginEnabled => prefs.enableSmartHierarchy;
        
        private static GUIStyle sceneVisibilityStyle;
        
        protected override IEnumerable<Patch> GetPatches()
        {
            var type = EditorAssembly.GetType("UnityEditor.SceneVisibilityHierarchyGUI");
            
            var doItemGUI = AccessTools.Method(type, "DoItemGUI");
            var drawBackground = AccessTools.Method(type, "DrawBackground");
            var drawItemBackground = AccessTools.Method(type, "DrawItemBackground");
            var sceneHeaderOverflow = AccessTools.PropertyGetter(type, "k_sceneHeaderOverflow");
            
            yield return new Patch(doItemGUI, nameof(DoItemGUI));
            yield return new Patch(drawBackground, nameof(DrawBackground));
            yield return new Patch(drawItemBackground, nameof(DrawItemBackground));
            yield return new Patch(sceneHeaderOverflow, nameof(SceneHeaderOverflowGetter));
        }

        private static bool SceneHeaderOverflowGetter(ref float __result)
        {
            __result = 0;
            return false;
        }
        
        public static void DoItemGUI(ref Rect rect)
        {
            if (!pluginEnabled)
                return;

            if (sceneVisibilityStyle == null)
            {
                sceneVisibilityStyle = "SceneVisibility";

                sceneVisibilityStyle.fixedHeight = 0;
                sceneVisibilityStyle.stretchHeight = true;
                sceneVisibilityStyle.alignment = TextAnchor.MiddleCenter;
            }
        }
        
        public static bool DrawBackground(ref Rect rect)
        {
            if (!pluginEnabled)
                return true;

            return true;
        }
        
        public static bool DrawItemBackground(ref Rect rect)
        {
            if (!pluginEnabled)
                return true;
            
            return true;
        }
    }
}