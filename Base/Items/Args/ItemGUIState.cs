using System;

namespace AV.Hierarchy
{
    [Flags]
    public enum ItemGUIState : byte
    {
        Normal = 0,
        Selected = 1,
        Hovered = 2,
        Focused = 4,
        On = 8
    }
}