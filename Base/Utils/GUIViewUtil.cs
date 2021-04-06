using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static System.Linq.Expressions.Expression;

namespace AV.Hierarchy
{
    [InitializeOnLoad]
    public static class GUIViewUtil
    {
        static Type guiViewType = typeof(Editor).Assembly.GetType("UnityEditor.GUIView");
        static Type guiClipType = typeof(GUIUtility).Assembly.GetType("UnityEngine.GUIClip");
        
        static PropertyInfo currentViewInfo = guiViewType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
        static MethodInfo markHotRegionInfo = guiViewType.GetMethod("MarkHotRegion", BindingFlags.NonPublic | BindingFlags.Instance);
        
        static MethodInfo unclipToWindowInfo = guiClipType.GetMethod("UnclipToWindow", BindingFlags.Public | BindingFlags.Static,
            null, new [] { typeof(Rect) }, null);

        static Func<object> getCurrentView;
        static Action<Rect> markHotRegion;
        static Func<Rect, Rect> unclipToWindow;
        
        static GUIViewUtil()
        {
            var objParam = Parameter(typeof(object));
            var rectParam = Parameter(typeof(Rect));
            var convertToView = Convert(Property(null, currentViewInfo), guiViewType);
            
            markHotRegion = Lambda<Action<Rect>>(Call(convertToView, markHotRegionInfo, rectParam), rectParam).Compile();
            unclipToWindow = Lambda<Func<Rect, Rect>>(Call(unclipToWindowInfo, rectParam), rectParam).Compile();
        }

        public static void MarkHotRegion(Rect rect)
        {
            rect = unclipToWindow(rect);
            markHotRegion(rect);
        }
    }
}
