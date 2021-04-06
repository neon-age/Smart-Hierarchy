using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class GameObjectTreeViewGUIPatch
    {
        private static GUIContent tempContent = new GUIContent();
        
        private static GUIStyle selectionStyle;
        //private static Texture2D selectionActiveImage;
        
        private static readonly Color WhiteColor = new Color(1, 1, 1, 1);
        
        internal static void Initialize()
        {
            var harmony = new Harmony(nameof(TreeViewGUIPatch));
            
            var goGuiType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectTreeViewGUI");

            var drawItemBackground = goGuiType.GetMethod("DrawItemBackground", BindingFlags.NonPublic| BindingFlags.Instance);
            var onAdditionalGUI = goGuiType.GetMethod("OnAdditionalGUI", BindingFlags.NonPublic| BindingFlags.Instance);
            var onContentGUI = goGuiType.GetMethod("OnContentGUI", BindingFlags.NonPublic| BindingFlags.Instance);
            
            harmony.Patch(drawItemBackground, postfix: GetPatch("DrawItemBackground"));
            harmony.Patch(onAdditionalGUI, postfix: GetPatch("OnAdditionalGUI"));
            
            harmony.Patch(onContentGUI, prefix: GetPatch("OnContentGUI_Prefix"));
            harmony.Patch(onContentGUI, postfix: GetPatch("OnContentGUI_Postfix"));
        }

        private static HarmonyMethod GetPatch(string methodName)
        {
            return new HarmonyMethod(typeof(GameObjectTreeViewGUIPatch).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static));
        }

        private static void DrawItemBackground(object __instance, Rect rect, int row, TreeViewItem item, bool selected, bool focused)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            if (!SmartHierarchy.Instances.TryGetValue(__instance, out var hierarchy))
                return;
            
            if (selectionStyle == null)
                selectionStyle = "TV Selection";
            
            hierarchy.TryGetOrCreateItem(item, out var viewItem);

            var contentIndent = viewItem.itemArgs.contentIndent;
            var hovered = hierarchy.hoveredItem == item;
            var itemState = viewItem.GetItemGUIState(hovered, selected, focused);
            
            var args = new ItemGUIArgs
            {
                item = item, row = row, 
                rect = rect, contentIndent = contentIndent, 
                state = itemState
            };

            //if (selected && focused)
                viewItem.DoItemBackground(ref args);
            
            if (selected)
                selectionStyle.Draw(rect, false, false, true, focused);
        }
        
        private static void OnAdditionalGUI(object __instance, ref float ___m_ContentRectRight, Rect rect, int row, TreeViewItem item, bool selected, bool focused)
        {
            if (!SmartHierarchy.Instances.TryGetValue(__instance, out var hierarchy))
                return;
            
            hierarchy.TryGetOrCreateItem(item, out var viewItem);

            var contentIndent = viewItem.itemArgs.contentIndent;
            var hovered = hierarchy.hoveredItem == item;
            var itemState = viewItem.GetItemGUIState(hovered, selected, focused);

            tempContent.text = item.displayName;
            var labelWidth = hierarchy.gui.GetLineStyle().CalcSize(tempContent).x;

            // PrefabModeButton rect is assigned in GameObjectItem, so we ignore it there
            ___m_ContentRectRight = rect.xMax;
            
            var args = new AdditionalGUIArgs
            {
                rowRect = rect, 
                contentIndent = contentIndent, 
                labelWidth = labelWidth,
                state = itemState
            };
            
            viewItem.DoAdditionalGUI(ref args);

            ___m_ContentRectRight -= args.GetUsedRightSpace();
        }
        
        private static void OnContentGUI_Prefix(object __instance, ref Rect rect, int row, TreeViewItem item, string label, bool selected, bool focused, 
            ref bool useBoldFont, bool isPinging)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            if (!SmartHierarchy.Instances.TryGetValue(__instance, out var hierarchy))
                return;

            hierarchy.TryGetOrCreateItem(item, out var viewItem);

            var gui = hierarchy.controller.gui;
            var contentIndent = viewItem.itemArgs.contentIndent;
            var iconRect = rect;

            if (!isPinging)
            {
                iconRect.xMin += contentIndent + gui.GetExtraSpaceBeforeIcon();
            }

            var iconArgs = new IconGUIArgs {
                rect = iconRect, 
                icon = viewItem.effectiveIcon,
                overlayIcon = viewItem.overlayIcon,
                isOn = selected && focused,
                color = Color.white
            };
            var labelArgs = new LabelGUIArgs {
                rect = rect, 
                boldFont = useBoldFont, 
                color = Color.white
            };
            
            viewItem.DoIconGUI(ref iconArgs, isPinging);
            
            if (viewItem is GameObjectItemBase goItem)
            {
                GameObjectItemUtil.SetOverlayIcon(item, iconArgs.overlayIcon);
                
                if (useBoldFont && goItem.renderDisabled)
                    labelArgs.color *= new Color(1, 1, 1, 0.5f);
            }
            
            viewItem.DoLabelGUI(ref labelArgs);
            
            rect = labelArgs.rect;
            useBoldFont = labelArgs.boldFont;
            GUI.contentColor = labelArgs.color;

            GUI.color = WhiteColor;
            GUI.backgroundColor = WhiteColor;
        }

        private static void OnContentGUI_Postfix(Rect rect, int row, TreeViewItem item, string label, bool selected, bool focused,
            bool useBoldFont, bool isPinging)
        {
            GUI.contentColor = WhiteColor;
        }
    }
}
