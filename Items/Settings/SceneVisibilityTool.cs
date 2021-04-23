using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AV.Hierarchy
{
    [Serializable]
    internal class SceneVisibilityTool : HierarchyTool
    {
        public bool drawBackground;

        protected internal override int order => 0;
        protected internal override string title => "Scene Visibility"; 
        protected internal override string tooltip => "Hide and show game-objects in Scene View.";
        //protected internal override Texture2D icon => GetEditorIcon("scenevis_visible");
        protected internal override Texture2D icon => GetEditorIcon("animationvisibilitytoggleon");

        public override void OnValidate()
        {
            options.GetTool<ScenePickingTool>().drawBackground = drawBackground;
            options.Save();
        }
    }
}
