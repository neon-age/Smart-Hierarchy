using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(Light))]
    public class LightItem : GameObjectItemBase
    {
        public override int order => 0;
        public override int preservedColumnSpace => 16;

        private static readonly Texture2D dotIcon = LoadAssetFromGUID<Texture2D>("398a9944e21ee3f48b739c044a4eeeef");
        
        private readonly Light light;
        
        public LightItem(Light component) : base(component)
        {
            this.light = component;
        }

        public override void OnAdditionalGUI(ref AdditionalGUIArgs args)
        {
            var columnRect = args.GetPreservedColumnRectAndClip(16);
            
            if (IsRectHovered(columnRect) || args.isSelected)
            {
                var colorRect = columnRect;
                colorRect = Padding(colorRect, 1);
                
                EditorGUI.ColorField(colorRect, GUIContent.none, light.color, false, true, false);
            }
            else
            {
                //colorRect.xMin += 16;
                //
                var dotRect = new Rect(columnRect) { width = 4, height = 4 };
                dotRect = GetCenteredRect(dotRect, columnRect);
                
                DrawIcon(dotRect, dotIcon, light.color);
            }
        }
    }
}
