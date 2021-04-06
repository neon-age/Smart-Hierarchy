

using System;

namespace AV.Hierarchy
{
    [Flags]
    internal enum HierarchyItemOverrides
    {
        Nothing = 0,
        OnBeforeIcon = 1,
        OnBeforeLabel = 2,
        OnBeforeBackground = 4,
        OnBeforeAdditionalGUI = 8,
        OnDrawBackground = 16,
        OnDrawIcon = 32,
        OnIconClick = 64,
        OnItemGUI = 128,
        OnAdditionalGUI = 256,
        OnHandleUnusedEvents = 512
    }
}