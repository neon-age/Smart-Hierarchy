using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    [InitializeOnLoad]
    internal static class SceneHierarchyWindowPatch
    {
        private static Texture2D filterIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("15da02c18233ca44f89e8255124491de"));
        private static GUIContent buttonContent = new GUIContent();
        private static GUIStyle dropdownStyle;
        
        static SceneHierarchyWindowPatch()
        {
            var type = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            
            var harmony = new Harmony("SceneHierarchyWindow Patch");

            var doToolbarLayout = type.GetMethod("DoToolbarLayout", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefix = typeof(SceneHierarchyWindowPatch).GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static);
            var postfix = typeof(SceneHierarchyWindowPatch).GetMethod("Postfix", BindingFlags.Public | BindingFlags.Static);

            harmony.Patch(doToolbarLayout, new HarmonyMethod(prefix), new HarmonyMethod(postfix));
        }

        public static void Prefix()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
        }

        public static void Postfix()
        {
            if (dropdownStyle == null)
                dropdownStyle = new GUIStyle("ToolbarCreateAddNewDropDown")
                {
                    padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(2, 2, 0, 0), 
                    border = new RectOffset(0, 2, 0, 0) 
                };
            
            GUILayout.FlexibleSpace();
            
            GUILayout.BeginHorizontal();
            
            buttonContent.image = filterIcon;
            buttonContent.tooltip = "Filter options";
            
            var rect = GUILayoutUtility.GetRect(34, 18);
            //rect.yMin += 2;
            
            if (EditorGUI.DropdownButton(rect, buttonContent, FocusType.Passive, dropdownStyle))
            {
                var filterPopup = new HierarchyOptionsPopup();

                var position = rect.position;
                position.x += 1;
                position.y += 32;
                
                filterPopup.ShowInsideWindow(position, SmartHierarchy.active.root);
            }
            
            GUILayout.EndHorizontal();
        }
    }
}
