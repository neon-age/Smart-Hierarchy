

using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class TreeViewGUIPatch : HarmonyPatchProvider<TreeViewGUIPatch>
    {
        private static HierarchyOptions options => HierarchyOptions.instance;

       
        [InitializeOnLoadMethod]
        static void OnLoad() => Initialize();
        
        protected override void OnInitialize()
        {
            var type = typeof(TreeView).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewGUI");

            var getFoldoutIndent = type.GetMethod("GetFoldoutIndent", BindingFlags.Public | BindingFlags.Instance);
            var getEffectiveIcon = type.GetMethod("GetEffectiveIcon", BindingFlags.NonPublic | BindingFlags.Instance);
            
            Patch(getFoldoutIndent, postfix: nameof(GetFoldoutIndent));
            Patch(getEffectiveIcon, postfix: nameof(GetEffectiveIcon_Postfix));
        }

        private static void GetFoldoutIndent(ref float __result, TreeViewItem item)
        {
            // Preserve space for ActivationToggle, but ignore root scene header.
            if (item.depth > 0)
            {
                __result += SmartHierarchy.GetOffsetForCustomIndentWidth(1);
            }
        }
        
        private static void GetEffectiveIcon_Postfix(TreeViewItem item)
        {
            var hierarchy = SmartHierarchy.active;
            if (hierarchy == null)
                return;
            
            var lineStyle = hierarchy.gui.GetLineStyle();

            lineStyle.fixedHeight = 0;
            lineStyle.alignment = TextAnchor.MiddleLeft;
        }
    }
}