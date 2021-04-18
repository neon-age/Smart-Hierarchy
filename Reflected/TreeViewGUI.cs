using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class TreeViewGUI
    {
        private object instance;
        
        private static Type type;
        private static FieldInfo baseIndentField;
        private static FieldInfo indentWidthField;
        private static FieldInfo iconWidthField;
        private static FieldInfo iconSpaceField;
        private static FieldInfo halfDropBetweenHeightField;
        
        private static PropertyInfo lineHeightProperty;
        private static PropertyInfo lineStyleProperty;
        private static PropertyInfo foldoutStyleProperty;
        private static PropertyInfo spaceBeforeIconProperty;
        
        private static Func<object, float> getIndentWidth;
        private static Action<object, float> setIndentWidth;
        
        private static MethodInfo isSubSceneHeader;

        private static GUIStyle foldout;

        private readonly float defaultIconWidth;
        private readonly float defaultSpaceBeforeIcon;

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            type = typeof(TreeView).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewGUI");

            baseIndentField = type.GetField("k_BaseIndent");
            indentWidthField = type.GetField("k_IndentWidth");
            iconWidthField = type.GetField("k_IconWidth");
            iconSpaceField = type.GetField("k_SpaceBetweenIconAndText");
            halfDropBetweenHeightField = type.GetField("k_HalfDropBetweenHeight");
            
            lineHeightProperty = type.GetProperty("k_LineHeight");
            lineStyleProperty = type.GetProperty("lineStyle", BindingFlags.NonPublic | BindingFlags.Instance);
            foldoutStyleProperty = type.GetProperty("foldoutStyle", BindingFlags.NonPublic | BindingFlags.Instance);
            spaceBeforeIconProperty = type.GetProperty("extraSpaceBeforeIconAndLabel");
            
            isSubSceneHeader = typeof(Editor).Assembly.GetType("SubSceneGUI").GetMethod("IsSubSceneHeader", BindingFlags.NonPublic | BindingFlags.Static);
        }

        public TreeViewGUI(object instance)
        {
            this.instance = instance;
            
            defaultIconWidth = (float)iconWidthField.GetValue(instance);
            defaultSpaceBeforeIcon = (float)iconSpaceField.GetValue(instance);
        }

        public static bool IsSubSceneHeader(GameObject gameObject)
        {
            return (bool)isSubSceneHeader.Invoke(null, new object[] { gameObject });
        }

        public void SetLineHeight(float height)
        {
            lineHeightProperty.SetValue(instance, height);
        }

        public GUIStyle GetLineStyle()
        {
            return lineStyleProperty.GetValue(instance) as GUIStyle;
        }

        public void ResetCustomStyling()
        {
            iconWidthField.SetValue(instance, defaultIconWidth);
            iconSpaceField.SetValue(instance, defaultSpaceBeforeIcon);
        }
        
        public void SetBaseIndent(float indent)
        {
            baseIndentField.SetValue(instance, indent);
        }
        public float GetBaseIndent()
        {
            return (float)baseIndentField.GetValue(instance);
        }
        
        public void SetIndentWidth(float indent)
        {
            indentWidthField.SetValue(instance, indent);
        }
        
        public void SetIconWidth(float width)
        {
            iconWidthField.SetValue(instance, width);
        }

        public void SetSpaceBetweenIconAndText(float space)
        {
            iconSpaceField.SetValue(instance, space);
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