using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(GameObject))]
    public class TagItem : GameObjectItemBase
    {
        public override int order => -25;
        
        private static readonly Texture2D tagIcon = LoadAssetFromGUID<Texture2D>("7ff0f1889c60ed64a855228c57d0163f");
        //private static readonly Texture2D tagIcon = GetEditorIconContent("FilterByLabel");
        
        public TagItem(GameObject gameObject) : base(gameObject)
        {
        }

        public override void OnAdditionalGUI(ref AdditionalGUIArgs args)
        {
            if (gameObject.CompareTag("Untagged"))
                return;
            
            var rect = args.GetContentRectAndClip(16);
            
            var color = args.GetRectColor(rect);
            
            DrawGrayIcon(rect, tagIcon, color, args.isOn);
        }
    }
}
