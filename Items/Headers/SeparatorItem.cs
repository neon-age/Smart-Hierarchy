using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(Separator))]
    public class SeparatorItem : GameObjectItemBase
    {
        private Separator separator;
        
        public SeparatorItem(Separator component) : base(component)
        {
            this.separator = component;
        }

        protected internal override void OnAfterCreation()
        {
            if (TryGetItem<ActivationToggleItem>(out var activationToggle))
                activationToggle.enabled = false;
        }

        public override void OnBeforeIcon(ref IconGUIArgs args)
        {
            args.isHidden = true;
            args.icon = null;
        }

        public override void OnDrawBackground(ItemGUIArgs args)
        {
            var rect = new Rect(args.rect);

            var height = 4;
            rect.y += rect.height / 2 - height;
            rect.height = 4;
            
            EditorGUI.DrawRect(rect, Color.gray);
        }
    }
}
