using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    public class EditorPopupWindow : EditorWindow
    {
        float maxWidth;
        float maxHeight;

        Rect lastButtonRect;
        
        public void CreateAt(Rect buttonRect, Vector2 windowSize, bool closeOnLostFocus = false)
        {
            lastButtonRect = buttonRect;
            position = this.ShowAsDropdownFitToScreen(buttonRect, windowSize);

            this.ShowPopupWithMode(WindowShowMode.PopupMenu, false);
            
            position = this.ShowAsDropdownFitToScreen(buttonRect, windowSize);
            
            minSize = new Vector2(position.width, position.height);
            maxSize = new Vector2(position.width, position.height);
            
            if (closeOnLostFocus)
                this.AddToAuxWindowList();
            
            Repaint();
            this.DontSaveToLayout();
        }

        public void ShowAt(Rect buttonRect, Vector2 windowSize)
        {
            position = this.ShowAsDropdownFitToScreen(lastButtonRect, windowSize);

            SetSize(windowSize);
            
            position = this.ShowAsDropdownFitToScreen(buttonRect, windowSize);

            lastButtonRect = buttonRect;
        }

        public void SetSize(Vector2 size)
        {
            var width = maxWidth == 0 ? size.x : maxWidth;
            var height = maxHeight == 0 ? size.y : maxHeight;
            
            minSize = new Vector2(width, height);
            maxSize = new Vector2(width, height);
        }

        public void SetMaxHeight(float height)
        {
            maxHeight = height;
            maxSize = new Vector2(position.width, height);
        }
    }
}
