

using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class TreeViewControllerPatch : HarmonyPatchBase<TreeViewControllerPatch>
    {
        protected override void OnInitialize()
        {
            var controllerType = typeof(Editor).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");

            var handleUnusedEvents = controllerType.GetMethod("HandleUnusedMouseEventsForItem", BindingFlags.Public | BindingFlags.Instance);

            Patch(handleUnusedEvents, prefix: nameof(HandleUnusedMouseEventsForItem_Prefix));
        }
        
        private static void HandleUnusedMouseEventsForItem_Prefix(object __instance, Rect rect, TreeViewItem item, int row)
        {
            var gui = TreeViewController.GetTreeViewGUI(__instance);

            if (!SmartHierarchy.Instances.TryGetValue(gui, out var hierarchy))
                return;

            hierarchy.TryGetOrCreateItem(item, out var viewItem);
            
            var args = new ItemEventArgs
            {
                rect = rect,
                row = row,
                controlID = GetItemControlID(item)
            };
            
            viewItem.DoHandleUnusedEvents(args);
        }
        
        private static void HandleUnusedMouseEventsForItem_Postfix(object __instance, Rect rect, TreeViewItem item, int row)
        {
            var gui = TreeViewController.GetTreeViewGUI(__instance);

            if (!SmartHierarchy.Instances.TryGetValue(gui, out var hierarchy))
                return;

            hierarchy.TryGetOrCreateItem(item, out var viewItem);
            
            var args = new ItemEventArgs
            {
                rect = rect,
                row = row,
                controlID = GetItemControlID(item)
            };
            
            viewItem.DoHandleUnusedEvents(args);
        }
        
        private static int GetItemControlID(TreeViewItem item)
        {
            return (item?.id ?? 0) + 10000000;
        }
    }
}