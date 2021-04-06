
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    /// <summary>
    /// Used as a root item for all game-objects in SceneHierarchy.
    /// </summary>
    internal class GameObjectItem : GameObjectItemBase
    {
        private static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;

        public override int order => -1000;

        public new readonly bool isRootPrefab;
        public readonly Texture2D gizmoIcon;
        public readonly ComponentsList components;
        
        private static SwipeToggle<int> swiping = new SwipeToggle<int>(2);

        
        public GameObjectItem(GameObject gameObject) : base(gameObject)
        {
            components = new ComponentsList(gameObject);
            
            isRootPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(gameObject);
            
            icon = components.icon;
            gizmoIcon = ObjectIconUtil.GetObjectGizmoIcon(gameObject);

            targetType = components.main?.GetType() ?? typeof(GameObject);
        }

        public override void OnBeforeIcon(ref IconGUIArgs args)
        {
            overlayIcon = GameObjectItemUtil.GetOverlayIcon(view);
            
            if (renderDisabled)
                args.color *= new Color(1f, 1f, 1f, 0.5f);
        }

        public override void OnBeforeAdditionalGUI(ref AdditionalGUIArgs args)
        {
            // Prefab Mode Button column
            args.PreserveSpaceForColumn(16);
        }

        public override void OnAdditionalGUI(ref AdditionalGUIArgs args)
        {
            if (showPrefabModeButton)
                args.GetPreservedColumnRectAndClip(16);

            if (!args.isHovered)
                return;

            var foldoutRect = itemArgs.rect;
            
            /*
            var foldoutRect = new Rect(args.indentedRect) { width = args.labelWidth + 22 };
            foldoutRect.x -= 4;*/

            if (swiping.DoVerticalToggle(foldoutRect, IsExpanded()))
            {
                if (CanBeExpanded())
                    ChangeExpandedState();
            }
            
            return;

            if (!foldoutRect.Contains(evt.mousePosition))
                return;

            var button = evt.button;

            if (button == 2 && evt.type == EventType.MouseDown)
            {
                //evt.type = EventType.MouseDrag;
                //evt.clickCount = 2;
                if (CanBeExpanded())
                {
                    ChangeExpandedState();
                    evt.Use();
                }
            }
        }

        protected override Texture2D GetEffectiveIcon()
        {
            if (gizmoIcon != null && prefs.showGizmoIcon)
                return gizmoIcon;
            
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