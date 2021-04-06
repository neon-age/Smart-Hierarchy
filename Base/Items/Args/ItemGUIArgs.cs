using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AV.Hierarchy
{
    public struct ItemGUIArgs
    {
        public TreeViewItem item;
        
        public int row;
        public Rect rect;
        public float contentIndent;

        public ItemGUIState state;
        
        public bool isSelected => (state & ItemGUIState.Selected) != 0;
        public bool isHovered => (state & ItemGUIState.Hovered) != 0;
        public bool isFocused => (state & ItemGUIState.Focused) != 0;

        public ItemGUIArgs(ItemGUIArgs args)
        {
            this = args;
        }
    }
}
