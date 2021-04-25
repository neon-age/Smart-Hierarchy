using System;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class ScenePickingTool : HierarchyTool
    {
        public bool drawBackground = true;
        
        protected internal override int order => 1;
        protected internal override string title => "Scene Picking"; 
        protected internal override string tooltip => "Game-objects pickability in Scene.";
        //protected internal override Texture2D icon => GetEditorIcon("scenepicking_pickable");
        protected internal override Texture2D icon => GetEditorIcon("scenepicking_pickable_hover");
        
        public override void OnBeforeSave()
        {
            options.GetTool<SceneVisibilityTool>().drawBackground = drawBackground;
        }
    }
}