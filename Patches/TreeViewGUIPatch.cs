using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class TreeViewGUIPatch : HarmonyPatchBase<TreeViewGUIPatch>
    {
        private static Texture2D indentIcon;
        
        protected override void OnInitialize()
        {
            var guiType = typeof(Editor).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewGUI");

            var onRowGUI = guiType.GetMethod("OnRowGUI", BindingFlags.Public | BindingFlags.Instance);
            var getEffectiveIcon = guiType.GetMethod("GetEffectiveIcon", BindingFlags.NonPublic| BindingFlags.Instance);
            
            Patch(onRowGUI, prefix: nameof(OnRowGUI_Prefix));
            Patch(getEffectiveIcon, prefix: nameof(GetEffectiveIcon_Prefix));
        }
        
        private static void OnRowGUI_Prefix(object __instance, Rect rowRect, TreeViewItem item, int row, bool selected, bool focused)
        {
            if (!SmartHierarchy.Instances.TryGetValue(__instance, out var hierarchy))
                return;

            hierarchy.TryGetOrCreateItem(item, out var viewItem);

            if (!viewItem.IsItemValid())
            {
                hierarchy.RemoveItem(item.id);
                return;
            }

            var contentIndent = hierarchy.gui.GetContentIndent(item);
            var hovered = hierarchy.hoveredItem == item;
            var itemState = viewItem.GetItemGUIState(hovered, selected, focused);

            rowRect.xMin += contentIndent;
            
            var args = new ItemGUIArgs
            {
                item = item, row = row, 
                rect = rowRect, contentIndent = contentIndent, 
                state = itemState
            };

            viewItem.DoItemGUI(args);
        }

        private static bool GetEffectiveIcon_Prefix(object __instance, ref Texture __result, TreeViewItem item)
        {
            if (!SmartHierarchy.Instances.TryGetValue(__instance, out _))
                return true;
            
            if (indentIcon == null)
            {
                indentIcon = new Texture2D(1, 1);
                indentIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
                indentIcon.Apply();
            }
            
            __result = indentIcon;
            
            // Case for Missing Prefab dummies where GO icon is missing
            if (!item.icon)
                __result = null;              
            
            return false;
        }
    }
}
