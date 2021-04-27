using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class HierarchyToolbarPatch : HarmonyPatchProvider<HierarchyToolbarPatch>
    {
        internal static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;
        
        private static Texture2D filterIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("15da02c18233ca44f89e8255124491de"));
        private static GUIContent buttonContent = new GUIContent { tooltip = "Options" };
        private static GUIStyle dropdownStyle;
        private static GUIStyle toolbarButtonRight;

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
            if (!prefs.enableSmartHierarchy)
                return;
            
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
        }

        private static void DoToolbarLayout_Postfix()
        {
            if (!prefs.enableSmartHierarchy)
                return;

            if (dropdownStyle == null)
            {
                dropdownStyle = new GUIStyle("ToolbarCreateAddNewDropDown")
                {
                    padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(2, 2, 0, 0),
                    border = new RectOffset(0, 2, 0, 0)
                };
                toolbarButtonRight = new GUIStyle("toolbarbuttonRight");
            }

            //GUILayout.FlexibleSpace();
            
            buttonContent.image = filterIcon;
            
            var rect = GUILayoutUtility.GetRect(32, 18, toolbarButtonRight);
            //rect.yMin += 2;

            using (new GUIContentColorScope(GUIColors.FlatIcon))
            {
                if (EditorGUI.DropdownButton(rect, buttonContent, FocusType.Passive, dropdownStyle))
                {
                    var filterPopup = new HierarchyOptionsPopup();

                    var position = rect.position;
                    position.x += 1;
                    position.y += 31;

                    filterPopup.ShowInsideWindow(position, SmartHierarchy.active.window.actualWindow);
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}
