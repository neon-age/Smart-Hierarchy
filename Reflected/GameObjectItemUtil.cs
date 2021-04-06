using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    // https://github.com/Unity-Technologies/UnityCsReference/blob/2019.4/Editor/Mono/GUI/TreeView/GameObjectTreeViewItem.cs
    internal static class GameObjectItemUtil
    {
        private static Type goItemType;
        
        private static Func<TreeViewItem, int> getColorCode;
        private static Func<TreeViewItem, bool> getShowPrefabModeButton;
        private static Func<TreeViewItem, Texture2D> getOverlayIcon;
        private static Action<TreeViewItem, Texture2D> setOverlayIcon;

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            goItemType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectTreeViewItem");

            var colorCodeInfo = goItemType.GetProperty("colorCode");
            var overlayIconInfo = goItemType.GetProperty("overlayIcon");
            var getShowPrefabModeButtonInfo = goItemType.GetProperty("showPrefabModeButton");

            var itemParam = Expression.Parameter(typeof(TreeViewItem));
            var textureParam = Expression.Parameter(typeof(Texture2D));
            var itemConvert = Expression.Convert(itemParam, goItemType);
            
            getColorCode = Expression.Lambda<Func<TreeViewItem, int>>(Expression.Property(itemConvert, colorCodeInfo), itemParam).Compile();
           
            getOverlayIcon = Expression.Lambda<Func<TreeViewItem, Texture2D>>(Expression.Property(itemConvert, overlayIconInfo), itemParam).Compile();
            setOverlayIcon = Expression.Lambda<Action<TreeViewItem, Texture2D>>(Expression.Property(itemConvert, overlayIconInfo), itemParam, textureParam).Compile();
            
            getShowPrefabModeButton = Expression.Lambda<Func<TreeViewItem, bool>>(Expression.Property(itemConvert, getShowPrefabModeButtonInfo), itemParam).Compile();
        }
        
        public static int GetColorCode(TreeViewItem item)
        {
            return getColorCode.Invoke(item);
        }
        
        public static bool GetShowPrefabModeButton(TreeViewItem item)
        {
            return getShowPrefabModeButton.Invoke(item);
        }
        
        public static Texture2D GetOverlayIcon(TreeViewItem item)
        {
            return getOverlayIcon.Invoke(item);
        }

        public static void SetOverlayIcon(TreeViewItem item, Texture2D icon)
        {
            setOverlayIcon.Invoke(item, icon);
        }
    }
}