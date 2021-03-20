using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    public class GameObjectItem : HierarchyItem
    {
        private static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;
        
        public readonly bool isRootPrefab;
        public readonly GameObject gameObject;
        public readonly ComponentsList components;
        public readonly Texture2D gizmoIcon;

        private GameObjectViewItem goView;
        
        private static readonly Texture2D NullComponentIcon = IconContent("DefaultAsset Icon").image as Texture2D;
        
        
        public GameObjectItem(Component component) : this(component.gameObject) {}
        
        public GameObjectItem(GameObject instance) : base(instance.GetInstanceID(), instance)
        {
            gameObject = instance;
            
            isRootPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(instance);
            
            components = new ComponentsList(instance);
            
            gizmoIcon = ObjectIconUtil.GetIconForObject(instance);
            icon = components.icon;
            
            if (prefs.showGizmoIcon && gizmoIcon != null)
                icon = gizmoIcon;
            
            if (components.hasNullComponent)
                icon = NullComponentIcon;
            
            targetType = components.main?.GetType() ?? typeof(GameObject);
        }

        protected override void OnViewUpdate()
        {
            goView = new GameObjectViewItem(view);
            overlayIcon = goView.overlayIcon;
        }

        public override void OnItemGUI()
        {
            this.DoActivationToggle(rect, isHover);
            
            var renderDisabled = goView.colorCode >= 4;
            
            if (renderDisabled)
                tintColor *= new Color(1f, 1f, 1f, 0.5f);

            base.OnItemGUI();
        }

        protected override Texture2D GetEffectiveIcon()
        {
            if (components.hasNullComponent)
                return NullComponentIcon;
            
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
            
            return base.GetEffectiveIcon();
        }
    }
}
