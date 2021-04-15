using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class SceneHierarchyWindowPatch : HarmonyPatchProvider<SceneHierarchyWindowPatch>
    {
        private static Texture2D filterIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("15da02c18233ca44f89e8255124491de"));
        private static GUIContent buttonContent = new GUIContent();
        private static GUIStyle dropdownStyle;

        [InitializeOnLoadMethod]
        static void OnLoad() => Initialize();
        
        protected override void OnInitialize()
        {
            var type = EditorAssembly.GetType("UnityEditor.SceneHierarchyWindow");
            
            var doToolbarLayout = type.GetMethod("DoToolbarLayout", BindingFlags.NonPublic | BindingFlags.Instance);

            Patch(doToolbarLayout, nameof(DoToolbarLayout_Prefix), nameof(DoToolbarLayout_Postfix));
        }

        private static void DoToolbarLayout_Prefix()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
        }

        private static void DoToolbarLayout_Postfix()
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
