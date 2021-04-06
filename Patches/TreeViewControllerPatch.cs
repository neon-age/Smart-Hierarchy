

using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    public class TreeViewControllerPatch
    {
        internal static void Initialize()
        {
            var harmony = new Harmony(nameof(TreeViewGUIPatch));
            
            var controllerType = typeof(Editor).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewController");

            var handleUnusedEvents = controllerType.GetMethod("HandleUnusedMouseEventsForItem", BindingFlags.Public | BindingFlags.Instance);
            
            harmony.Patch(handleUnusedEvents, prefix: GetPatch("HandleUnusedMouseEventsForItem"));
        }

        private static HarmonyMethod GetPatch(string methodName)
        {
            return new HarmonyMethod(typeof(TreeViewGUIPatch).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static));
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