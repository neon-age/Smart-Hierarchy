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
        
        private static MethodInfo isSubSceneHeader;

        private static GUIStyle foldout;

        private readonly float defaultIconWidth;
        private readonly float defaultSpaceBeforeIcon;

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
            
            isSubSceneHeader = typeof(Editor).Assembly.GetType("SubSceneGUI").GetMethod("IsSubSceneHeader", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public TreeViewGUI(object gui)
        {
            this.gui = gui;
            
            defaultIconWidth = (float)iconWidthField.GetValue(gui);
            defaultSpaceBeforeIcon = (float)iconSpaceField.GetValue(gui);
        }

        public static bool IsSubSceneHeader(GameObject gameObject)
        {
            return (bool)isSubSceneHeader.Invoke(null, new object[] { gameObject });
        }

        public void SetLineHeight(float height)
        {
            lineHeightProperty.SetValue(gui, height);
        }

        public void ResetCustomStyling()
        {
            iconWidthField.SetValue(gui, defaultIconWidth);
            iconSpaceField.SetValue(gui, defaultSpaceBeforeIcon);
        }

        public void SetIconWidth(float width)
        {
            iconWidthField.SetValue(gui, width);
        }

        public void SetSpaceBetweenIconAndText(float space)
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