#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static System.Linq.Expressions.Expression;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal class SmartHierarchy
    {
        internal static SmartHierarchy lastHierarchy;
        
        internal SceneHierarchyWindow window { get; }

        internal SceneHierarchy hierarchy => window.hierarchy;
        
        private readonly Dictionary<int, ViewItem> ItemsData = new Dictionary<int, ViewItem>();
        
        
        public SmartHierarchy(object window)
        {
            this.window = new SceneHierarchyWindow(window);
            
            RegisterCallbacks();
            hierarchy.ReassignCallbacks();
        }

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUI;
        }

        private void RegisterCallbacks()
        {
            HierarchySettingsProvider.onChange += ReloadView;
            HierarchySettingsProvider.onChange += ImmediateRepaint;
            
            Selection.selectionChanged += ReloadView;
            hierarchy.onVisibleRowsChanged += ReloadView;
            EditorApplication.hierarchyChanged += ReloadView;
        }

        private void ReloadView()
        {
            ItemsData.Clear();
        }
        
        private void ImmediateRepaint()
        {
            EditorApplication.DirtyHierarchyWindowSorting();
        }

        private static void OnHierarchyItemGUI(int instanceId, Rect rect)
        {
            lastHierarchy = HierarchyInitialization.GetLastHierarchy();
            
            var preferences = HierarchySettingsProvider.Preferences;
            
            if (!preferences.enableSmartHierarchy)
                return;
            
            lastHierarchy.OnItemGUI(instanceId, rect);
        }
        
        private void OnItemGUI(int id, Rect rect)
        {
            var instance = EditorUtility.InstanceIDToObject(id) as GameObject;

            if (!instance)
                return;
            
            GetInstanceViewItem(id, instance, out var item);
            
            // Happens to be null when entering prefab mode
            if (!item.EnsureViewExist(hierarchy))
                return;
            
            var fullWidthRect = GetFullWidthRect(rect);
            
            item.UpdateViewIcon();

            OnOverlayGUI(fullWidthRect, item);
        }
        
        private void GetInstanceViewItem(int id, GameObject instance, out ViewItem item)
        {
            if (!ItemsData.TryGetValue(id, out item))
            {
                item = new ViewItem(instance);
               
                ItemsData.Add(id, item);
            }
        }

        private void OnOverlayGUI(Rect rect, ViewItem item)
        {
            var evt = Event.current;
            var isHovering = rect.Contains(evt.mousePosition);
            var instance = item.instance;

            if (isHovering)
            {
                var toggleRect = new Rect(rect) { x = 32 };
                if (OnLeftToggle(toggleRect, instance.activeSelf, out var isActive))
                {
                    Undo.RecordObject(instance, "GameObject Set Active");
                    instance.SetActive(isActive);
                }
            }
        }

        private static Rect GetFullWidthRect(Rect rect)
        {
            var fullWidthRect = new Rect(rect);
            fullWidthRect.x = 0;
            fullWidthRect.width = Screen.width;
            return fullWidthRect;
        }

        private static bool OnLeftToggle(Rect rect, bool isActive, out bool value)
        {
            var toggleRect = new Rect(rect) { width = 16 };
            
            EditorGUI.BeginChangeCheck();
            value = GUI.Toggle(toggleRect, isActive, GUIContent.none);
            return EditorGUI.EndChangeCheck();
        }
    }
}
#endif
