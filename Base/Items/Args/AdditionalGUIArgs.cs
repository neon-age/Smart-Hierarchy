using UnityEngine;

namespace AV.Hierarchy
{
    public struct AdditionalGUIArgs
    {
        public Rect rowRect;
        public Rect indentedRect => new Rect(rowRect) { xMin = contentIndent };
        
        public float contentIndent;
        public float labelWidth;

        /// This is a rough solution, needs to be rewritten into proper Toolbar API.
        /// Clips label in <seealso cref="GameObjectTreeViewGUIPatch.OnAdditionalGUI"/>
        internal float preservedColumnSpace;
        internal float usedPreservedSpace;
        internal float usedContentSpace;
        internal float usedRightSpace;
        
        public ItemGUIState state;
        
        public bool isSelected => (state & ItemGUIState.Selected) != 0;
        public bool isHovered => (state & ItemGUIState.Hovered) != 0;
        public bool isFocused => (state & ItemGUIState.Focused) != 0;
        public bool isOn => (state & ItemGUIState.On) != 0;

        
        public bool IsRectHovered(Rect rect)
        {
            if (isHovered)
                GUIViewUtil.MarkHotRegion(rect);
            return rect.Contains(Event.current.mousePosition);
        }
        
        public Color GetRectColor(Rect rect)
        {
            var color = Color.white;

            color.a = IsRectHovered(rect) ? 1 : isFocused ? 0.825f : isHovered ? 0.65f : 0.5f;
            
            return color;
        }

        internal float GetUsedRightSpace()
        {
            if (usedContentSpace > 0)
                return preservedColumnSpace + usedContentSpace;

            if (usedPreservedSpace == 0)
                return 0;

            return Mathf.Clamp(usedRightSpace, 0, preservedColumnSpace);
        }
        
        public Rect GetContentRect(float width)
        {
            var rect = rowRect;
            
            rect.xMax -= usedContentSpace + preservedColumnSpace;
            rect.xMin = rect.xMax - width;
            
            return rect;
        }
        
        public Rect GetContentRectAndClip(float width)
        {
            var rect = GetContentRect(width);
            usedContentSpace += width;
            return rect;
        }
        
        public Rect GetPreservedColumnRectAndClip(float width)
        {
            usedPreservedSpace += width;
            
            var rect = rowRect;
            
            var xMin = rect.xMax - usedPreservedSpace - width;
            rect.xMin = xMin;
            rect.xMax -= usedPreservedSpace;

            if (usedRightSpace < xMin)
                usedRightSpace = rect.xMin;
            
            return rect;
        }
        
        public void PreserveSpaceForColumn(float space)
        {
            var rect = rowRect;
            rect.xMax -= preservedColumnSpace;
            rect.xMin = rect.xMax - space;
            
            preservedColumnSpace += space;
        }
    }
}