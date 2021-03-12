using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal enum WindowShowMode
    {
        NormalWindow,
        PopupMenu,
        Utility,
        NoShadow,
        MainWindow,
        AuxWindow,
        Tooltip,
        ModalUtility,
    }
    
    internal enum PopupLocation
    {
        Below = 0,
        BelowAlignLeft = 0,
        Above = 1,
        AboveAlignLeft = 1,
        Left = 2,
        Right = 3,
        Overlay = 4,
        BelowAlignRight = 5,
        AboveAlignRight = 6,
    }
    
    internal static class EditorWindowUtils
    {
        static Type hostViewType = typeof(Editor).Assembly.GetType("UnityEditor.HostView");
        static Type popupLocationType = typeof(Editor).Assembly.GetType("UnityEditor.PopupLocation");
        static Type containerWindowType = typeof(Editor).Assembly.GetType("UnityEditor.ContainerWindow");
        
        static FieldInfo getHostView = typeof(EditorWindow).GetField("m_Parent", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo getRootView = typeof(EditorWindow).GetField("m_RootView", BindingFlags.NonPublic | BindingFlags.Instance) ??
                                       typeof(EditorWindow).GetField("m_MainView", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo dontSaveToLayoutField = containerWindowType.GetField("m_DontSaveToLayout", BindingFlags.NonPublic | BindingFlags.Instance);
        
        static PropertyInfo getContainerWindow = hostViewType.GetProperty("window", BindingFlags.Public | BindingFlags.Instance);
        
        static MethodInfo addToAuxWindowList = typeof(EditorWindow).GetMethod("AddToAuxWindowList", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo showPopupWithMode = typeof(EditorWindow).GetMethod("ShowPopupWithMode", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo containerShowPopupWithMode = containerWindowType.GetMethod("ShowPopupWithMode", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo fitWindowRectToScreen = containerWindowType.GetMethod("FitWindowRectToScreen", BindingFlags.Public | BindingFlags.Instance); 
        static MethodInfo fitToScreen = typeof(EditorWindow).GetMethod("ShowAsDropDownFitToScreen", BindingFlags.NonPublic | BindingFlags.Instance, null, 
            new [] { typeof(Rect), typeof(Vector2), popupLocationType.MakeArrayType() }, null);

        static MethodInfo repaintImmediately = typeof(EditorWindow).GetMethod("RepaintImmediately", BindingFlags.NonPublic | BindingFlags.Instance);

        
        static object GetHostView(this EditorWindow window)
        {
            return getHostView.GetValue(window);
        }
        
        static object GetContainerWindow(this EditorWindow window)
        {
            return getContainerWindow.GetValue(window.GetHostView());
        }

        
        public static void RepaintImmediately(this EditorWindow window)
        {
            repaintImmediately.Invoke(window, null);
        }

        public static void DontSaveToLayout(this EditorWindow window)
        {
            dontSaveToLayoutField.SetValue(window.GetContainerWindow(), true);
        }

        public static void AddToAuxWindowList(this EditorWindow window)
        {
            addToAuxWindowList.Invoke(window.GetHostView(), null);
        }

        public static void ShowPopupWithMode(this EditorWindow window, WindowShowMode showMode, bool giveFocus)
        {
            showPopupWithMode.Invoke(window, new object[] { (int)showMode, giveFocus });
        }
        
        public static Rect FitWindowRectToScreen(this EditorWindow window, Rect rect, bool forceCompletelyVisible = true, bool useMouseScreen = false)
        {
            return (Rect)fitWindowRectToScreen.Invoke(window.GetContainerWindow(), new object[] { rect, forceCompletelyVisible, useMouseScreen });
        }
        
        public static Rect ShowAsDropdownFitToScreen(this EditorWindow window, Rect buttonRect, Vector2 windowSize)
        {
            return (Rect)fitToScreen.Invoke(window, new object[] { buttonRect, windowSize, null });
        }
        
        public static Rect ShowAsDropdownFitToScreen(this EditorWindow window, Rect buttonRect, Vector2 windowSize, PopupLocation location)
        {
            return (Rect)fitToScreen.Invoke(window, new object[] { buttonRect, windowSize, new [] { (int)location } });
        }
    }
}
