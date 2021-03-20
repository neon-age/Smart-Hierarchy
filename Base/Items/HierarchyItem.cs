using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEditor.AssetDatabase;
using static UnityEditor.EditorGUIUtility;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    public abstract class HierarchyItem
    {
        public Type targetType { get; protected internal set; } = typeof(Object);
        public Texture2D icon  { get; protected set; } 
        public Texture2D overlayIcon { get; protected set; }
        public Texture2D effectiveIcon => GetEffectiveIcon() ?? view.icon;
        
        protected Rect rect { get; private set; }
        protected Color tintColor { get; set; }
        protected bool isOn { get; private set; }
        protected bool isHover { get; private set; }
        protected bool isFocused { get; private set; }
        
        public TreeViewItem view { get; private set; }
        
        public readonly Object instance;
        public readonly int id;
        
        public readonly bool isPrefab;
        
        
        public HierarchyItem(int id, Object instance = default)
        {
            if (instance == null)
                instance = EditorUtility.InstanceIDToObject(id);

            this.id = id;
            this.instance = instance;

            if (instance != null)
            {
                isPrefab = PrefabUtility.GetPrefabAssetType(instance) == PrefabAssetType.Regular;
            }
        }
        
        public virtual void OnItemGUI()
        {
            this.DrawIcon(rect, tintColor, isOn);
        }

        protected virtual HierarchyItem CreateForInstance(Object instance) => null;
        protected virtual HierarchyItem CreateForInstance(int id) => null;
        
        protected void DrawIcon(Rect rect, Color color, bool isOn)
        {
            HierarchyItemGUI.DrawIcon(this, rect, color, isOn);
        }

        protected bool OnIconClick(Rect rect)
        {
            return HierarchyItemGUI.OnIconClick(rect);
        }
        
        protected virtual Texture2D GetEffectiveIcon() => view.icon;
        
        protected virtual void OnViewUpdate() {}
        
        internal bool EnsureViewExist(SceneHierarchy hierarchy)
        {
            if (view == null)
            {
                view = hierarchy.GetViewItem(id);
                if(view == null)
                    return false;
            }
            
            OnViewUpdate();
            
            return true;
        }
        
        internal void DoItemGUI(HierarchyItemArgs args)
        {
            rect = args.rect;
            tintColor = GUI.color;
            
            isOn = args.on;
            isHover = args.hovered;
            isFocused = args.focused;

            OnItemGUI();
        }
    }
}