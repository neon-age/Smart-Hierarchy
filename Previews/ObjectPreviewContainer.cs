using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEngine.UIElements.PopupWindow;

namespace AV.Hierarchy
{ 
    public class ObjectPreviewContainer : VisualElement
    {
        public ObjectPreviewBase preview;
        private IMGUIContainer container;

        public ObjectPreviewContainer()
        {
            style.position = Position.Relative;
            
            container = new IMGUIContainer(() =>
            {
                if (preview == null)
                    return;
                
                var guiStyle = container.resolvedStyle;
                
                var color = GUI.color;
                GUI.color = new Color(1, 1, 1, resolvedStyle.opacity * guiStyle.opacity);

                if (preview.IsCached)
                {
                    var cachedPreview = preview.GetCachedPreview(preview.TargetID);

                    GUI.DrawTexture(preview.RenderArea, cachedPreview, ScaleMode.ScaleAndCrop, true);
                }

                GUI.color = color;
            });
            Add(container);
        }
    }
}
