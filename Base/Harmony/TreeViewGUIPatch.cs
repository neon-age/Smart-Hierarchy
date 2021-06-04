using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class TreeViewGUIPatch : PatchBase
    {
         protected override IEnumerable<Patch> GetPatches()
         {
             var guiType = typeof(Editor).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewGUI");
             
             var onRowGUI = AccessTools.Method(guiType, "OnRowGUI");
             var getEffectiveIcon = AccessTools.Method(guiType, "GetEffectiveIcon");

             yield return new Patch(onRowGUI, prefix: nameof(_OnRowGUI));
             yield return new Patch(getEffectiveIcon, prefix: nameof(_GetEffectiveIcon));
         }
         
         private static void _OnRowGUI(object __instance, Rect rowRect, TreeViewItem item, int row, bool selected, bool focused)
         {
         }
         
         private static bool _GetEffectiveIcon(object __instance, ref Texture __result, TreeViewItem item)
         {
             return true;
         }
    }
}
