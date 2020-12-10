using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    // https://github.com/Unity-Technologies/UnityCsReference/blob/2019.4/Editor/Mono/GUI/TreeView/GameObjectTreeViewItem.cs
    internal class GameObjectViewItem
    {
        private static Type type;
        private static PropertyInfo colorCodeProperty;
        private static PropertyInfo overlayIconProperty;

        public int colorCode;
        public Texture2D overlayIcon;

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            type = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectTreeViewItem");

            colorCodeProperty = type.GetProperty("colorCode");
            overlayIconProperty = type.GetProperty("overlayIcon");
        }

        public GameObjectViewItem(TreeViewItem item)
        {
            colorCode = (int)colorCodeProperty.GetValue(item);
            overlayIcon = overlayIconProperty.GetValue(item) as Texture2D;
        }
    }
}