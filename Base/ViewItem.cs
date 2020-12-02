using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal class ViewItem
    {
        internal Rect rect;
        internal TreeViewItem view;
        
        internal readonly int id;
        internal readonly GameObject instance;
        internal readonly Collection collection;
        internal readonly Transform transform;
        internal readonly Components components;
        internal readonly Type mainType; 
        internal readonly Texture2D icon;
        
        internal readonly ViewItem child;
        
        internal readonly bool isPrefab;
        internal readonly bool isRootPrefab;
        internal readonly bool isCollection;
        internal readonly bool isEmpty;

        private static Texture2D transparent;
        private static Texture2D folderIcon = IconContent("Folder On Icon").image as Texture2D;
        private static Texture2D folderEmptyIcon = IconContent("FolderEmpty On Icon").image as Texture2D;
        private static Texture2D collectionsIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("6ee527fd28545e04593219b473dc26da"));

        public ViewItem(GameObject instance)
        {
            this.instance = instance;
            
            id = instance.GetInstanceID();
            
            transform = instance.transform;
            components = new Components(instance);

            icon = components.icon;

            isPrefab = PrefabUtility.GetPrefabAssetType(instance) == PrefabAssetType.Regular;
            isRootPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(instance);
            isCollection = instance.TryGetComponent(out collection);
            isEmpty = instance.transform.childCount == 0;
            
            mainType = components.main?.GetType() ?? typeof(GameObject);

            if (isRootPrefab)
                mainType = typeof(GameObject);

            if (!isEmpty)
                child = new ViewItem(transform.GetChild(0).gameObject);
        }
        
        public bool EnsureViewExist(SceneHierarchy hierarchy)
        {
            if (view == null)
            {
                view = hierarchy.GetViewItem(id);
                if(view == null)
                    return false;
            }

            return true;
        }
        
        public void DrawFolderIcon(Rect rect, bool isSelected)
        {
            var iconRect = new Rect(rect) { width = 16, height = 16 };

            var guiColor = GUI.color;
            var tintColor =  ColorTags.GetColor(collection.colorTag);

            var icon = instance.transform.childCount == 0 ? folderEmptyIcon : folderIcon;
            
            if (!isSelected)
                GUI.color *= tintColor;
            
            if (!instance.activeInHierarchy)
                GUI.color *= new Color(1, 1, 1, 0.5f);
            
            GUI.DrawTexture(iconRect, collectionsIcon);
            
            GUI.color = guiColor;
        }
        
        public void UpdateViewIcon()
        {
            var preferences = HierarchySettingsProvider.Preferences;
            
            if (isCollection)
            {
                if (transparent == null)
                {
                    transparent = new Texture2D(1, 1);
                    transparent.SetPixel(0, 0, new Color(1, 1, 1, 0));
                    transparent.Apply();
                }
                
                // Draw icon manually, as there is no way to tint it without touching text or background
                view.icon = transparent;
            }
            else
            {
                if (icon == null) 
                    return;
                
                switch (preferences.stickyComponentIcon)
                {
                    case StickyIcon.Never: 
                        break;
                    case StickyIcon.OnAnyObject:
                        view.icon = icon;
                        break;
                    case StickyIcon.NotOnPrefabs:
                        if (!isRootPrefab)
                            view.icon = icon;
                        break;
                }
            }
        }
    }
}