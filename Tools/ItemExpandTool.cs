using System;
using UnityEngine;

namespace AV.Hierarchy
{
    public class ItemExpandTool : HierarchyTool
    {
        public bool swiping;
        public bool useMiddleClick;
        
        protected internal override string title => "Quick Expanding"; 
        protected internal override string tooltip => "";
        protected internal override Texture2D icon => UIResources.Index.foldoutIcon;
    }
}