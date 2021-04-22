using System;
using UnityEngine;

namespace AV.Hierarchy
{
    [Serializable]
    public class ItemExpandTool : HierarchyTool
    {
        public bool swiping;
        public bool useMiddleClick;
        
        protected internal override string title => "Quick Expanding"; 
        protected internal override string tooltip => "";
        protected internal override Texture2D icon => LoadAssetFromGUID<Texture2D>("ca7ad4c62f74dc042b3a35b034fa031c");
    }
}