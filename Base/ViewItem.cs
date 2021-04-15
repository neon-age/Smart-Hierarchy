using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.AssetDatabase;
using static UnityEditor.EditorGUIUtility;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal class ViewItem
    {
        private static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;
        
        public TreeViewItem view;
        private GameObjectViewItem goView;

        public int colorCode => goView.colorCode;
        public Texture2D overlayIcon => goView.overlayIcon;
        public Texture2D effectiveIcon => GetEffectiveIcon() ?? view.icon;
        
        public readonly int id;
        public readonly Object instance;
        public readonly GameObject gameObject;
        public readonly Collection collection;
        public readonly Components components;
        public readonly Type mainType; 
        public readonly Texture2D icon;
        public readonly Texture2D gizmoIcon;
        
        public readonly bool isPrefab;
        public readonly bool isRootPrefab;
        public readonly bool isCollection;
        public readonly bool isSceneHeader;
        public readonly bool isSubSceneHeader;
        
        private static MethodInfo getSceneByHandleInfo = typeof(EditorSceneManager).GetMethod("GetSceneByHandle", BindingFlags.NonPublic | BindingFlags.Static);
        private static Func<int, Scene> getSceneHandle = Delegate.CreateDelegate(typeof(Func<int, Scene>), getSceneByHandleInfo) as Func<int, Scene>;
       
        private static readonly Texture2D collectionIcon = LoadAssetAtPath<Texture2D>(GUIDToAssetPath("6ee527fd28545e04593219b473dc26da"));
        private static readonly Texture2D nullComponentIcon = IconContent("DefaultAsset Icon").image as Texture2D;
        private static readonly Texture2D sceneAssetIcon = IconContent("SceneAsset Icon").image as Texture2D;
        
        public ViewItem(int id, Object instance = null)
        {
            var hasInstance = instance != null;
            
            this.instance = instance;
            this.mainType = typeof(Object);
            
            if (hasInstance)
                id = instance.GetInstanceID();

            this.id = id;
            
            isSceneHeader = getSceneHandle.Invoke(id).IsValid();
            
            gizmoIcon = ObjectIconUtil.GetIconForObject(instance);
           
            if (prefs.showGizmoIcon && gizmoIcon != null)
                icon = gizmoIcon;

            if (isSceneHeader || isSubSceneHeader)
                icon = sceneAssetIcon;

            if (instance is GameObject gameObject)
            {
                this.gameObject = gameObject;
                
                isPrefab = PrefabUtility.GetPrefabAssetType(gameObject) == PrefabAssetType.Regular;
                isRootPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(gameObject);
                isCollection = gameObject.TryGetComponent(out collection);
                isSubSceneHeader = TreeViewGUI.IsSubSceneHeader(gameObject);

                components = new Components(gameObject);
                
                icon = components.icon;
                
                if (components.hasNullComponent)
                    icon = nullComponentIcon;
                
                mainType = components.main?.GetType() ?? typeof(GameObject);
            }
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
            
            switch (prefs.effectiveIcon)
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