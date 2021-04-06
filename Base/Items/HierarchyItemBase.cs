using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEditor.AssetDatabase;
using static UnityEditor.EditorGUIUtility;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    public abstract class HierarchyItemBase
    {
        public virtual int order => 0;
        public virtual int preservedColumnSpace => 0;
        
        protected internal Event evt => Event.current;
        
        public TreeViewItem view { get; internal set; } = HierarchyItemCreator.currentTreeViewItem;
        public Type targetType { get; protected internal set; } = typeof(Object);

        public Texture2D icon  { get; protected set; } 
        public Texture2D overlayIcon { get; protected set; }
        public Texture2D effectiveIcon => GetEffectiveIcon() ?? view.icon;

        public ItemGUIArgs itemArgs { get; private set; }
        public TreeViewGUIArgs viewArgs { get; private set; }

        public bool enabled = true;
        
        public readonly int id;
        public readonly Object instance;
        
        internal readonly HierarchyItemBase root = HierarchyItemCreator.currentRootItem;
        
        internal List<HierarchyItemBase> stack;
        internal HierarchyItemOverrides overrides;
        
        
        public HierarchyItemBase(int id, Object instance = default)
        {
            this.id = id;
            this.instance = instance;
            
            stack = root == null ? new List<HierarchyItemBase> { this } : root.stack;
            
            if (root == null)
                root = this;
        }
        
        protected void CancelCreation() => HierarchyItemCreator.cancelCreation = true;

        
        #region Virtual Methods
        protected internal virtual void OnAfterCreation()
        {
            /*
            var log = "";
            foreach (var child in root.stack)
            {
                log += " > " + child.GetType().Name;
            }
            Debug.Log(log);*/
        }

        protected internal virtual bool IsItemValid()
        {
            return true;
        }
        
        protected virtual HierarchyItemBase CreateForInstance(Object instance) => null;
        protected virtual HierarchyItemBase CreateForInstance(int id) => null;
        
        public virtual void OnBeforeIcon(ref IconGUIArgs args) {}
        public virtual void OnBeforeLabel(ref LabelGUIArgs args) {}
        public virtual void OnBeforeBackground(ref ItemGUIArgs args) {}
        public virtual void OnBeforeAdditionalGUI(ref AdditionalGUIArgs args) {}
        public virtual void OnBeforeHandleUnusedEvents(ItemEventArgs args) {}
        
        public virtual void OnDrawBackground(ItemGUIArgs args) {}
        public virtual void OnDrawIcon(IconGUIArgs args) => DrawIconDefault(args);
        public virtual bool OnIconClick(IconGUIArgs args) => false;
        public virtual void OnHandleUnusedEvents(ItemEventArgs args) {}
        
        public virtual void OnItemGUI(ItemGUIArgs args) {}
        public virtual void OnAdditionalGUI(ref AdditionalGUIArgs args) {}

        protected virtual Texture2D GetEffectiveIcon() => view.icon;
        #endregion

        
        #region Items Query
        public HierarchyItemBase GetItem<TItem>() where TItem : HierarchyItemBase
        {
            foreach (var item in root.stack)
                if (item.GetType() == typeof(TItem)) return item;
            return null;
        }

        public bool TryGetItem<TItem>(out HierarchyItemBase item) where TItem : HierarchyItemBase
        {
            item = GetItem<TItem>();
            return item != null;
        }
        #endregion
        
        #region Item Methods
        protected bool IsSelected()
        {
            return viewArgs.controller.IsSelected(view);
        } 
        
        protected void SelectItem(bool keepMultiSelection)
        {
            viewArgs.controller.SelectionClick(view, keepMultiSelection);
        }

        protected bool IsExpanded()
        {
            return viewArgs.controller.IsExpanded(view.id);
        } 
        
        protected bool CanBeExpanded()
        {
            return viewArgs.controller.IsExpandable(view);
        }
        
        protected void SetExpanded(bool expanded)
        {
            viewArgs.controller.SetUserExpanded(view, itemArgs.row, expanded);
        }
        
        protected void ChangeExpandedState()
        {
            if (CanBeExpanded())
                viewArgs.controller.SetUserExpanded(view, itemArgs.row, !IsExpanded());
        }
        #endregion
        
        #region Editor Utils
        protected static Texture2D GetEditorIconContent(string iconName)
        {
            return IconContent(iconName)?.image as Texture2D;
        }
        
        protected static TAsset LoadAssetFromGUID<TAsset>(string guid) where TAsset : Object
        {
            return LoadAssetAtPath<TAsset>(GUIDToAssetPath(guid));
        }
        #endregion
        
        #region Rect Utils
        protected static Rect GetCenteredRect(Rect targetRect, Rect area)
        {
            targetRect.x = area.x + (area.width / 2) - (targetRect.width / 2);
            targetRect.y = area.y + (area.height / 2) - (targetRect.height / 2);
            return targetRect;
        }

        protected static Rect Padding(Rect rect, float amount)
        {
            return Padding(rect, amount, amount, amount, amount);
        }
        
        protected static Rect Padding(Rect rect, float top, float left, float right, float bottom)
        {
            rect.yMin += top;
            rect.xMin += left;
            rect.xMax -= right;
            rect.yMax -= bottom;
            return rect;
        }
        #endregion

        #region GUI Methods
        protected bool IsRectHovered(Rect rect)
        {
            if (itemArgs.isHovered)
                GUIViewUtil.MarkHotRegion(rect);
            return rect.Contains(Event.current.mousePosition);
        }
        
        protected static int OnMouseDownEvent(Rect rect) => HierarchyItemGUI.OnMouseDownEvent(rect);
        protected static bool OnLeftClick(Rect rect) => HierarchyItemGUI.OnLeftClick(rect);
        protected static bool OnMiddleClick(Rect rect) => HierarchyItemGUI.OnMiddleClick(rect);
        protected static bool OnRightClick(Rect rect) => HierarchyItemGUI.OnRightClick(rect);
        
        private static void DrawIconDefault(IconGUIArgs args)
        {
            var rect = new Rect(args.rect) { width = 16, height = 16 };
            HierarchyItemGUI.DrawIcon(rect, args.icon, args.color, args.isOn);
        }

        protected static void DrawRect(Rect rect, Color color)
        {
            FastGUI.DrawRect(rect, color);
        }

        protected static void DrawGrayIcon(Rect rect, Texture2D icon, Color color, bool isOn = false)
        {
            var iconColor = isProSkin ? new Color(0.76f, 0.76f, 0.76f, 1f) : new Color(0.75f, 0.76f, 0.76f, 1f);

            if (isOn)
                iconColor = Color.white;
            
            iconColor *= color;
            
            HierarchyItemGUI.DrawIcon(rect, icon, iconColor, isOn);
        }
        protected static void DrawIcon(Rect rect, Texture2D icon, bool isOn = false)
        {
            HierarchyItemGUI.DrawIcon(rect, icon, Color.white, isOn);
        }
        protected static void DrawIcon(Rect rect, Texture2D icon, Color color, bool isOn = false)
        {
            HierarchyItemGUI.DrawIcon(rect, icon, color, isOn);
        }
        #endregion
        
        
        #region Internals
        internal void SetTreeViewData(TreeViewItem treeItem, TreeViewGUIArgs args)
        {
            foreach (var item in stack)
            {
                item.view = treeItem;
                item.viewArgs = args;
            }
        }
        
        internal ItemGUIState GetItemGUIState(bool hovered, bool selected, bool focused)
        {
            var itemState = ItemGUIState.Normal;
            itemState |= hovered ? ItemGUIState.Hovered : 0;
            itemState |= selected ? ItemGUIState.Selected : 0;
            itemState |= selected && focused ? ItemGUIState.Focused : 0;
            itemState |= selected && focused ? ItemGUIState.On : 0;
            return itemState;
        }
        
        internal bool HasOverride(HierarchyItemOverrides method)
        {
            return (overrides & method) != 0;
        }
        
        internal void DoItemGUI(ItemGUIArgs args)
        {
            try
            {
                for (var i = stack.Count - 1; i >= 0; i--)
                {
                    var item = stack[i];

                    if (!item.IsItemValid())
                    {
                        stack.RemoveAt(i);
                        continue;
                    }
                    
                    if (item.enabled)
                    {
                        item.itemArgs = args;
                        item.OnItemGUI(args);
                    }
                }
            }
            catch (Exception ex) { LogException(ex); }
        }
        
        internal void DoAdditionalGUI(ref AdditionalGUIArgs args)
        {
            try
            {
                for (var i = 0; i < stack.Count; i++)
                {
                    var item = stack[i];
                    if (item.enabled)
                    {
                        var preservedSpace = item.preservedColumnSpace;
                        
                        if (preservedSpace != 0)
                            args.PreserveSpaceForColumn(preservedSpace);
                        
                        item.OnBeforeAdditionalGUI(ref args);
                    }
                }
                
                for (var i = 0; i < stack.Count; i++)
                {
                    var item = stack[i];
                    if (item.enabled)
                        item.OnAdditionalGUI(ref args);
                }
            }
            catch (Exception ex) { LogException(ex); }
        }
        
        internal void DoIconGUI(ref IconGUIArgs args, bool isPinging)
        {
            try
            {
                HierarchyItemBase iconClickItem = null;
                HierarchyItemBase drawIconItem = root;

                for (var i = 0; i < stack.Count; i++)
                {
                    var item = stack[i];
                    if (item.enabled)
                    {
                        var rect = args.rect;
                        item.OnBeforeIcon(ref args);

                        if (!args.isHidden)
                        {
                            if (isPinging)
                                args.rect = rect;

                            if (HasOverride(HierarchyItemOverrides.OnDrawIcon))
                                drawIconItem = item;

                            if (HasOverride(HierarchyItemOverrides.OnIconClick))
                                iconClickItem = item;
                        }
                    }
                }

                drawIconItem.OnDrawIcon(args);
                
                if (iconClickItem != null && OnMouseDownEvent(args.rect) == 0)
                    iconClickItem.OnIconClick(args);
            }
            catch (Exception ex) { LogException(ex); }
        }
        
        internal void DoLabelGUI(ref LabelGUIArgs args)
        {
            try
            {
                for (var i = 0; i < stack.Count; i++)
                {
                    var item = stack[i];
                    if (item.enabled)
                    {
                        item.OnBeforeLabel(ref args);
                    }
                }
            }
            catch (Exception ex) { LogException(ex); }
        }
        
        internal void DoItemBackground(ref ItemGUIArgs args)
        {
            try
            {
                for (var i = 0; i < stack.Count; i++)
                {
                    var item = stack[i];
                    if (item.enabled)
                    {
                        item.OnBeforeBackground(ref args);
                        item.OnDrawBackground(args);
                    }
                }
            }
            catch (Exception ex) { LogException(ex); }
        }
        
        internal void DoHandleUnusedEvents(ItemEventArgs args)
        {
            try
            {
                var evt = Event.current;
                
                for (var i = 0; i < stack.Count; i++)
                {
                    var item = stack[i];
                    if (item.enabled)
                    {
                        FillItemEventArgs(ref args, evt);
                        
                        item.OnHandleUnusedEvents(args);
                    }
                }
            }
            catch (Exception ex) { LogException(ex); }
        }

        private static void FillItemEventArgs(ref ItemEventArgs args, Event evt)
        {
            args.type = evt.GetTypeForControl(args.controlID);
            args.button = evt.button;
            args.mousePosition = evt.mousePosition;
        }

        private static void LogException(Exception ex)
        {
            if (ex is ExitGUIException)
                return;
            Debug.LogException(ex);
        }
        #endregion
    }
}