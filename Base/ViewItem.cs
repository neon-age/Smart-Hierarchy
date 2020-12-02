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
        internal readonly Transform transform;
        internal readonly Components components;
        internal readonly Type mainType; 
        internal readonly Texture2D icon;
        
        internal readonly ViewItem child;
        
        internal readonly bool isPrefab;
        internal readonly bool isRootPrefab;
        internal readonly bool isFolder;
        internal readonly bool isEmpty;

        private static Texture2D collectionIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath("6ee527fd28545e04593219b473dc26da"));

        public ViewItem(GameObject instance)
        {
            this.instance = instance;
            
            id = instance.GetInstanceID();
            
            transform = instance.transform;
            components = new Components(instance);

            icon = components.icon;

            isPrefab = PrefabUtility.GetPrefabAssetType(instance) == PrefabAssetType.Regular;
            isRootPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(instance);
            isFolder = instance.TryGetComponent<Collection>(out _);
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
        
        public void UpdateViewIcon()
        {
            var preferences = HierarchySettingsProvider.Preferences;
            
            if (isFolder)
            {
                view.icon = collectionIcon;
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