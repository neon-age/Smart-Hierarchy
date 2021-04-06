using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class TreeViewGUI
    {
        public readonly object instance;
        
        private static Type type;
        private static FieldInfo iconWidthField;
        private static FieldInfo iconSpaceField;
        private static FieldInfo indentWidthField;
        private static FieldInfo halfDropBetweenHeightField;
        
        private static PropertyInfo lineHeightProperty;
        private static PropertyInfo foldoutStyleProperty;
        private static PropertyInfo spaceBeforeIconProperty;
        
        private static Func<GameObject, bool> isSubSceneHeader;
        private static Func<object, float> getBaseIndent;
        private static Func<object, float> getContentRectRight;
        private static Func<object, float> getExtraSpaceBeforeIcon;
        private static Func<object, TreeViewItem, float> getContentIndent;
        private static Action<object, GUIStyle> setSelectionStyle;
        private static Func<object, GUIStyle> getLineStyle;

        private static GUIStyle foldout;

        private readonly float defaultIconWidth;
        private readonly float defaultSpaceBeforeIcon;

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            type = typeof(TreeView).Assembly.GetType("UnityEditor.IMGUI.Controls.TreeViewGUI");

            iconWidthField = type.GetField("k_IconWidth");
            iconSpaceField = type.GetField("k_SpaceBetweenIconAndText");
            indentWidthField = type.GetField("k_IndentWidth");
            halfDropBetweenHeightField = type.GetField("k_HalfDropBetweenHeight");
            
            lineHeightProperty = type.GetProperty("k_LineHeight");
            foldoutStyleProperty = type.GetProperty("foldoutStyle", BindingFlags.NonPublic | BindingFlags.Instance);
            spaceBeforeIconProperty = type.GetProperty("extraSpaceBeforeIconAndLabel");
            
            var objParam = Expression.Parameter(typeof(object));
            var itemParam = Expression.Parameter(typeof(TreeViewItem));
            var styleParam = Expression.Parameter(typeof(GUIStyle));
            var convert = Expression.Convert(objParam, type);

            var selectionStyleInfo = type.GetProperty("selectionStyle", BindingFlags.NonPublic | BindingFlags.Instance);
            setSelectionStyle = Expression.Lambda<Action<object, GUIStyle>>(Expression.Call(convert, selectionStyleInfo.SetMethod, styleParam), objParam, styleParam).Compile();
            
            var lineStyleInfo = type.GetProperty("lineStyle", BindingFlags.NonPublic | BindingFlags.Instance);
            getLineStyle = Expression.Lambda<Func<object, GUIStyle>>(Expression.Call(convert, lineStyleInfo.GetMethod), objParam).Compile();
            
            var isSubSceneHeaderInfo = typeof(Editor).Assembly.GetType("SubSceneGUI").GetMethod("IsSubSceneHeader", BindingFlags.NonPublic | BindingFlags.Static);
            isSubSceneHeader = Delegate.CreateDelegate(typeof(Func<GameObject, bool>), isSubSceneHeaderInfo) as Func<GameObject, bool>;

            var getContentIndentInfo = type.GetMethod("GetContentIndent", BindingFlags.Public | BindingFlags.Instance);
            getContentIndent = Expression.Lambda<Func<object, TreeViewItem, float>>(Expression.Call(convert, getContentIndentInfo, itemParam), objParam, itemParam).Compile();
            
            var baseIndentField = type.GetField("k_BaseIndent", BindingFlags.Public | BindingFlags.Instance);
            getBaseIndent = Expression.Lambda<Func<object, float>>(Expression.Field(convert, baseIndentField), objParam).Compile();
            
            getExtraSpaceBeforeIcon = Expression.Lambda<Func<object, float>>(Expression.Property(convert, spaceBeforeIconProperty), objParam).Compile();
        }

        public TreeViewGUI(object instance)
        {
            this.instance = instance;
            
            defaultIconWidth = (float)iconWidthField.GetValue(instance);
            defaultSpaceBeforeIcon = (float)iconSpaceField.GetValue(instance);
        }
        
        public static bool IsSubSceneHeader(GameObject gameObject)
        {
            return isSubSceneHeader.Invoke(gameObject);
        }


        public void SetSelectionStyle(GUIStyle style)
        {
            setSelectionStyle.Invoke(instance, style);
        }
        
        public GUIStyle GetLineStyle()
        {
            return getLineStyle.Invoke(instance);
        }
        
        public float GetBaseIndent()
        {
            return getBaseIndent.Invoke(instance);
        }
        
        public float GetContentIndent(TreeViewItem item)
        {
            return getContentIndent.Invoke(instance, item);
        }
        
        public float GetExtraSpaceBeforeIcon()
        {
            return getExtraSpaceBeforeIcon.Invoke(instance);
        }

        public void SetLineHeight(float height)
        {
            lineHeightProperty.SetValue(instance, height);
        }
        
        public void SetIndentWidth(float width)
        {
            indentWidthField.SetValue(instance, width);
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