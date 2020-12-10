using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEditor.AssetDatabase;
using static UnityEditor.EditorGUIUtility;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal class ViewItem
    {
        private static HierarchyPreferences preferences = HierarchySettingsProvider.Preferences;
        
        public Rect rect;
        public TreeViewItem view;
        private GameObjectViewItem goView;

        public int colorCode => goView.colorCode;
        public Texture2D overlayIcon => goView.overlayIcon;
        public Texture2D effectiveIcon => GetEffectiveIcon() ?? view.icon;
        
        public readonly int id;
        public readonly GameObject instance;
        public readonly Collection collection;
        public readonly Transform transform;
        public readonly Components components;
        public readonly Type mainType; 
        public readonly Texture2D icon;
        
        public readonly ViewItem child;
        
        public readonly bool isPrefab;
        public readonly bool isRootPrefab;
        public readonly bool isCollection;
        public readonly bool isEmpty;
        
        private static Texture2D collectionIcon = LoadAssetAtPath<Texture2D>(GUIDToAssetPath("6ee527fd28545e04593219b473dc26da"));
        
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
            
            goView = new GameObjectViewItem(view);
            
            return true;
        }

        private Texture2D GetEffectiveIcon()
        {
            if (isCollection)
                return collectionIcon;
            
            switch (preferences.effectiveIcon)
            {
                case StickyIcon.Never: 
                    break;
                case StickyIcon.OnAnyObject:
                    return icon;
                case StickyIcon.NotOnPrefabs:
                    if (!isRootPrefab)
                        return icon;
                    break;
            }

            return view.icon;
        }
    }
}