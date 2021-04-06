using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(SpriteRenderer))]
    public class SpriteItem : GameObjectItemBase
    {
        private static readonly Texture2D spriteIcon = GetEditorIconContent("Sprite Icon");
        private static readonly Texture2D spriteRendererIcon = GetEditorIconContent("SpriteRenderer Icon");
    
        private SpriteRenderer sprite;
        
        public SpriteItem(SpriteRenderer sprite) : base(sprite)
        {
            this.sprite = sprite;
        }

        public override void OnBeforeIcon(ref IconGUIArgs args)
        {
            if (args.icon == spriteRendererIcon)
                args.icon = spriteIcon;
        }
    }
}
