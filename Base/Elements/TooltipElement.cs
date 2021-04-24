using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    // TODO: Implement hover delay or animation.
    internal class TooltipElement : VisualElement
    {
        public string text
        {
            get => textElement.text;
            set => textElement.text = value;
        }

        private TextElement textElement;
        
        private Dictionary<VisualElement, string> tooltips = new Dictionary<VisualElement, string>();
        

        public TooltipElement()
        {
            name = "Tooltip";
            visible = false;
            style.position = Position.Absolute;
            
            textElement = new TextElement();
            Add(textElement);
            
            RegisterCallback<GeometryChangedEvent>(_ => FitToParentBounds(textElement));

            textElement.style.alignSelf = Align.FlexStart;
            textElement.style.backgroundColor = isProSkin ? new Color(0.25f, 0.25f, 0.25f, 0.9f) : new Color(0.85f, 0.85f, 0.85f, 0.9f);

            textElement.style.SetBorderRadius(7);
            textElement.style.SetBorderWidth(1);
            textElement.style.SetBorderColor(new Color(0, 0, 0, 0.3f));
            textElement.style.SetPadding(4);
        }
        
        public void ShowAt(Vector2 mousePos, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                style.display = DisplayStyle.None;
                return;
            }
            style.display = DisplayStyle.Flex;
            
            this.text = text;

            style.top = mousePos.y;
            style.left = mousePos.x;
        }
        
        private void FitToParentBounds(VisualElement element)
        {
            var xMax = layout.xMax;
            var yMax = layout.yMax;
            
            var rootWidth = parent.layout.xMax;
            var rootHeight = parent.layout.yMax - layout.height;

            if (xMax > rootWidth)
                element.style.left = rootWidth - xMax;
            else
                element.style.left = 0;
            
            if (yMax > rootHeight)
                element.style.top = -element.layout.height * 2;
            else
                element.style.top = 0;
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            
        }

        public void SetTooltipFor(VisualElement element, string tooltip)
        {
            if (tooltips.TryGetValue(element, out _))
            {
                tooltips[element] = tooltip;
                return;
            }
            tooltips.Add(element, tooltip);
            
            element.RegisterCallback<MouseMoveEvent>(evt => ShowAt(evt.mousePosition, tooltips[element]));
            element.RegisterCallback<MouseLeaveEvent>(evt => visible = false);
            element.RegisterCallback<MouseEnterEvent>(evt =>
            {
                visible = true;
                ShowAt(evt.mousePosition, tooltip);
            });
        }
    }
}