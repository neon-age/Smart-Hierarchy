using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class SceneVisGUIPatch : HarmonyPatchProvider<SceneVisGUIPatch>
    {
        private static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;
        private static HierarchyOptions options => HierarchyOptions.Instance;
        
        private static bool pluginEnabled => prefs.enableSmartHierarchy;
        
        private static SceneVisibilityTool visibilityTool => options.GetTool<SceneVisibilityTool>();
        private static ScenePickingTool pickingTool => options.GetTool<ScenePickingTool>();

        private static bool visibility => options.IsToolEnabled<SceneVisibilityTool>();
        private static bool drawBackground => visibilityTool.drawBackground;
        private static bool picking => options.IsToolEnabled<ScenePickingTool>();

        private static GUIStyle sceneVisibilityStyle;
        
        
        protected override void OnInitialize()
        {
            var type = typeof(Editor).Assembly.GetType("UnityEditor.SceneVisibilityHierarchyGUI");
            
            var doItemGUI = type.GetMethod("DoItemGUI", BindingFlags.Public | BindingFlags.Static);
            var drawBackground = type.GetMethod("DrawBackground", BindingFlags.Public | BindingFlags.Static);
            var drawItemBackground = type.GetMethod("DrawItemBackground", BindingFlags.NonPublic | BindingFlags.Static);
            
            var drawGameObjectItemVisibility = type.GetMethod("DrawGameObjectItemVisibility", BindingFlags.NonPublic | BindingFlags.Static);
            var drawGameObjectItemPicking = type.GetMethod("DrawGameObjectItemPicking", BindingFlags.NonPublic | BindingFlags.Static);
            var drawSceneItemVisibility = type.GetMethod("DrawSceneItemVisibility", BindingFlags.NonPublic | BindingFlags.Static);
            var drawSceneItemPicking = type.GetMethod("DrawSceneItemPicking", BindingFlags.NonPublic | BindingFlags.Static);
            
            var sceneHeaderOverflow = type.GetProperty("k_sceneHeaderOverflow", BindingFlags.NonPublic | BindingFlags.Static);
            
            Patch(sceneHeaderOverflow.GetMethod, nameof(SceneHeaderOverflowGetter));
            
            Patch(doItemGUI, nameof(DoItemGUI));
            Patch(drawBackground, nameof(DrawBackground));
            Patch(drawItemBackground, nameof(DrawItemBackground));
            
            Patch(drawGameObjectItemVisibility, nameof(DrawGameObjectItemVisibility));
            Patch(drawGameObjectItemPicking, nameof(DrawGameObjectItemPicking));
            Patch(drawSceneItemVisibility, nameof(DrawSceneItemVisibility));
            Patch(drawSceneItemPicking, nameof(DrawSceneItemPicking));
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
                sceneVisibilityStyle = "SceneVisibility";

            sceneVisibilityStyle.fixedHeight = 0;
            sceneVisibilityStyle.stretchHeight = true;
            sceneVisibilityStyle.alignment = TextAnchor.MiddleCenter;
            //sceneVisibilityStyle.contentOffset = -new Vector2(0, 16 - options.layout.lineHeight);
            
            if (!visibility && picking)
                rect.xMin -= 16;
            
            if (!visibility && !picking)
                rect.xMin -= 32;
        }
        
        public static bool DrawBackground(ref Rect rect)
        {
            if (!pluginEnabled)
                return true;

            if (!visibility || !picking)
                rect.xMin -= 16;
            
            if (!visibility && !picking)
                rect.xMin -= 32;
            
            return drawBackground;
        }
        
        public static bool DrawItemBackground(ref Rect rect)
        {
            if (!pluginEnabled)
                return true;
            
            if (visibility && !picking)
                rect.xMin -= 16;
            
            return drawBackground;
        }
        
        public static bool DrawGameObjectItemVisibility()
        {
            if (!pluginEnabled)
                return true;
            return visibility;
        }
        
        public static bool DrawGameObjectItemPicking()
        {
            if (!pluginEnabled)
                return true;
            return picking;
        }
        
        public static bool DrawSceneItemVisibility()
        {
            if (!pluginEnabled)
                return true;
            return visibility;
        }
        
        public static bool DrawSceneItemPicking()
        {
            if (!pluginEnabled)
                return true;
            return picking;
        }
    }
}
