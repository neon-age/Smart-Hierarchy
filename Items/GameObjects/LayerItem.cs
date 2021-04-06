using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(GameObject))]
    public class LayerItem : GameObjectItemBase
    {
        public override int order => 0;
        
        private static readonly Texture2D layerIcon = LoadAssetFromGUID<Texture2D>("06bb7c5e381706f479a741b443f74b9f");
        //private static readonly Texture2D layerIcon = GetEditorIconContent("SceneViewFX");
        
        public LayerItem(GameObject gameObject) : base(gameObject)
        {
        }

        public override void OnAdditionalGUI(ref AdditionalGUIArgs args)
        {
            if (gameObject.layer == 0)
                return;
            
            var rect = args.GetContentRectAndClip(16);

            var color = args.GetRectColor(rect);
            
            DrawGrayIcon(rect, layerIcon, color, args.isOn);
            
            if (OnLeftClick(rect))
            {
                
            }
        }
    }
}
