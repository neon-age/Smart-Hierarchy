using UnityEngine;

namespace AV.Hierarchy
{
    [HierarchyItem(typeof(GameObject))]
    public class ActivationToggleItem : GameObjectItemBase
    {
        public ActivationToggleItem(GameObject gameObject) : base(gameObject)
        {
        }

        public override void OnAdditionalGUI(ref AdditionalGUIArgs args)
        {
            this.DoActivationToggle(args.rowRect, args.isHovered);
        }
    }
}