using UnityEngine;

namespace AV.Hierarchy
{
    public struct IconGUIArgs
    {
        public Texture2D icon;
        public Texture2D overlayIcon;
        
        public Rect rect;
        public Color color;
        public bool isHidden;
        public bool isOn;
    }
}