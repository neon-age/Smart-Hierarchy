using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(GameObject))]
    public class StaticItem : GameObjectItemBase
    {
        public override int order => 25;
        
        private static readonly Texture2D staticIcon = LoadAssetFromGUID<Texture2D>("09f660193ecec4c40ad4fa751cacf2ed");
        
        public StaticItem(GameObject gameObject) : base(gameObject)
        {
        }
        
        public override void OnAdditionalGUI(ref AdditionalGUIArgs args)
        {
            if (!gameObject.isStatic)
                return;

            var rect = args.GetContentRectAndClip(16);

            var color = args.GetRectColor(rect);
            
            DrawGrayIcon(rect, staticIcon, color, args.isOn);
            
            if (OnLeftClick(rect))
            {
                
            }
        }
    }
}
