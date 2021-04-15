using System;
using UnityEngine;

namespace AV.Hierarchy
{
    internal readonly struct GUIContentColorScope : IDisposable
    {
        private readonly Color guiColor;

        public GUIContentColorScope(Color color, bool condition = true, bool multiply = true)
        {
            guiColor = GUI.contentColor;
            
            if (!condition)
                return;
            
            if (multiply)
                GUI.contentColor *= color;
            else
                GUI.contentColor = color;
        }
        
        public void Dispose()
        {
            GUI.contentColor = guiColor;
        }
    }
}