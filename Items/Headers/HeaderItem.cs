using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;
using Debug = UnityEngine.Debug;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(Header))]
    internal class HeaderItem : GameObjectItemBase
    {
        public override int preservedColumnSpace => 16;

        private static Texture2D menuIcon = GetEditorIconContent("_Menu");
        
        private Header header;
        
        public HeaderItem(Header component) : base(component)
        {
            this.header = component;
        }

        protected internal override void OnAfterCreation()
        {
            if (TryGetItem<ActivationToggleItem>(out var activationToggle))
                activationToggle.enabled = false;
        }

        public override void OnHandleUnusedEvents(ItemEventArgs args)
        {
            var rect = args.rect;
            rect.xMin -= 4;

            //return;
            
            if (!rect.Contains(evt.mousePosition))
                return;
            
            var mouseClick = !itemArgs.isSelected ? evt.type == EventType.MouseDown : evt.type == EventType.MouseUp;
            
            if (mouseClick && evt.button == 0)
            {
                ChangeExpandedState();
                evt.Use();
            }

            if (evt.type == EventType.ContextClick)
            {
                if (args.IsHovered())
                    evt.Use();
            }
        }

        public override void OnItemGUI(ItemGUIArgs args)
        {
            
        }

        public override void OnBeforeIcon(ref IconGUIArgs args)
        {
            args.rect.xMin -= 14;
            args.icon = header.icon;
        }

        public override void OnBeforeLabel(ref LabelGUIArgs args)
        {
            args.rect.xMin -= 14;
            
            //if (!renderDisabled)
            //    args.color *= new Color(1, 1, 1, 0.85f);
            //else
            //    args.color *= new Color(1, 1, 1, 1.25f);
            
            args.boldFont = header.boldLabel;
        }

        public override void OnDrawBackground(ItemGUIArgs args)
        {
            var rect = new Rect(args.rect) { 
                xMin = args.contentIndent - 16,
                xMax = Screen.width, 
            };
            rect.yMax -= 1;

            var backgroundColor = isProSkin ? new Color(1, 1, 1, 1) : new Color(0, 0, 0, 0);
            backgroundColor.a = args.isHovered ? 0.133f : 0.1f;
            
            if (renderDisabled)
                backgroundColor.a *= 0.5f;

            var borderWidth = Vector4.one * 32;
            var borderRadius = Vector4.one * 3;
            
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, backgroundColor, borderWidth, borderRadius);
            //DrawRect(rect, backgroundColor);
        }
    }
}
