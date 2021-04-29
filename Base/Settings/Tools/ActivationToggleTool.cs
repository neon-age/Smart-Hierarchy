using System;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class ActivationToggleTool : HierarchyTool
    {
        public bool swiping;
        
        protected internal override int order => 10;
        protected internal override string title => "Activation Toggle"; 
        protected internal override string tooltip => "";
        protected internal override Texture2D icon => GetEditorIcon("Toggle Icon");
    }
}