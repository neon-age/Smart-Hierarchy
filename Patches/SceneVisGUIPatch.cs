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
    internal static class SceneVisGUIPatch
    {
        private static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;
        private static HierarchyOptions options => HierarchyOptions.instance;
        private static bool pluginEnabled => prefs.enableSmartHierarchy;
        private static bool visibility => options.showVisibilityToggle;
        private static bool picking => options.showPickingToggle;
        
        private static readonly Type type = typeof(Editor).Assembly.GetType("UnityEditor.SceneVisibilityHierarchyGUI");
        
        
        public static void Initialize()
        {
            var harmony = new Harmony("SceneVis GUI Patch");

            var doItemGUI = type.GetMethod("DoItemGUI", BindingFlags.Public | BindingFlags.Static);
            var drawBackground = type.GetMethod("DrawBackground", BindingFlags.Public | BindingFlags.Static);
            var drawItemBackground = type.GetMethod("DrawItemBackground", BindingFlags.NonPublic | BindingFlags.Static);
            
            var drawGameObjectItemVisibility = type.GetMethod("DrawGameObjectItemVisibility", BindingFlags.NonPublic | BindingFlags.Static);
            var drawGameObjectItemPicking = type.GetMethod("DrawGameObjectItemPicking", BindingFlags.NonPublic | BindingFlags.Static);
            var drawSceneItemVisibility = type.GetMethod("DrawSceneItemVisibility", BindingFlags.NonPublic | BindingFlags.Static);
            var drawSceneItemPicking = type.GetMethod("DrawSceneItemPicking", BindingFlags.NonPublic | BindingFlags.Static);

            
            harmony.Patch(doItemGUI, GetMethod(nameof(DoItemGUI)));
            harmony.Patch(drawBackground, GetMethod(nameof(DrawBackground)));
            harmony.Patch(drawItemBackground, GetMethod(nameof(DrawItemBackground)));
            
            harmony.Patch(drawGameObjectItemVisibility, GetMethod(nameof(DrawGameObjectItemVisibility)));
            harmony.Patch(drawGameObjectItemPicking, GetMethod(nameof(DrawGameObjectItemPicking)));
            harmony.Patch(drawSceneItemVisibility, GetMethod(nameof(DrawSceneItemVisibility)));
            harmony.Patch(drawSceneItemPicking, GetMethod(nameof(DrawSceneItemPicking)));
        }

        private static HarmonyMethod GetMethod(string methodName)
        {
            return new HarmonyMethod(typeof(SceneVisGUIPatch).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static));
        }
        
        public static void DoItemGUI(ref Rect rect)
        {
            if (!pluginEnabled)
                return;
            
            if (!visibility && picking)
                rect.xMin -= 16;
            
            if (!visibility && !picking)
                rect.xMin -= 32;
        }
        
        public static void DrawBackground(ref Rect rect)
        {
            if (!pluginEnabled)
                return;
            
            if (!visibility || !picking)
                rect.xMin -= 16;
            
            if (!visibility && !picking)
                rect.xMin -= 32;
        }
        
        public static void DrawItemBackground(ref Rect rect)
        {
            if (!pluginEnabled)
                return;
            
            if (visibility && !picking)
                rect.xMin -= 16;
        }
        
        public static bool DrawGameObjectItemVisibility()
        {
            if (!pluginEnabled)
                return true;
            return options.showVisibilityToggle;
        }
        
        public static bool DrawGameObjectItemPicking()
        {
            if (!pluginEnabled)
                return true;
            return options.showPickingToggle;
        }
        
        public static bool DrawSceneItemVisibility()
        {
            if (!pluginEnabled)
                return true;
            return options.showVisibilityToggle;
        }
        
        public static bool DrawSceneItemPicking()
        {
            if (!pluginEnabled)
                return true;
            return options.showPickingToggle;
        }
    }
}
