using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class TreeViewGUI
    {
        private object gui;
        
        private static Type type;
        private static FieldInfo iconWidthField;
        private static FieldInfo iconSpaceField;
        private static FieldInfo halfDropBetweenHeightField;
        
        private static PropertyInfo lineHeightProperty;
        private static PropertyInfo foldoutStyleProperty;
        private static PropertyInfo spaceBeforeIconProperty;
        
        
        private static GUIStyle foldout;

        private float defaultBaseIndent = -1;

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            type = typeof(TreeView).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewGUI");

            iconWidthField = type.GetField("k_IconWidth");
            iconSpaceField = type.GetField("k_SpaceBetweenIconAndText");
            halfDropBetweenHeightField = type.GetField("k_HalfDropBetweenHeight");
            
            lineHeightProperty = type.GetProperty("k_LineHeight");
            foldoutStyleProperty = type.GetProperty("foldoutStyle", BindingFlags.NonPublic | BindingFlags.Instance);
            spaceBeforeIconProperty = type.GetProperty("extraSpaceBeforeIconAndLabel");
        }

        public TreeViewGUI(object gui)
        {
            this.gui = gui;
        }

        public void SetLineHeight(float height)
        {
            lineHeightProperty.SetValue(gui, height);
        }

        public void SetIconWidth(float width)
        {
            iconWidthField.SetValue(gui, width);
        }

        public void SetSpaceBetweenIconAndText(int space)
        {
            iconSpaceField.SetValue(gui, space);
        }
        
        /*
        public void MakeFoldoutAreaBigger()
        {
            if (foldout == null)
            {
                foldout = new GUIStyle("IN Foldout");
                foldout.fixedWidth += 16;
            }

            foldoutStyleProperty.SetValue(gui, foldout);
            spaceBeforeIconProperty.SetValue(gui, -20);
        }*/
    }
}