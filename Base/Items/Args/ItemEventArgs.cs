

using UnityEngine;

namespace AV.Hierarchy
{
    public struct ItemEventArgs
    {
        public Rect rect;
        public int row;
        public int button;
        public int controlID;
        public EventType type;
        public Vector2 mousePosition;

        public bool IsHovered()
        {
            return rect.Contains(mousePosition);
        }
    }
}