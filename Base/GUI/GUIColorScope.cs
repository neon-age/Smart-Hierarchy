

using System;
using UnityEngine;

namespace AV.Hierarchy
{
    internal readonly struct GUIColorScope : IDisposable
    {
        private readonly Color guiColor;

        public GUIColorScope(Color color, bool condition = true, bool multiply = true)
        {
            guiColor = GUI.color;
            
            if (!condition)
                return;
            
            if (multiply)
                GUI.color *= color;
            else
                GUI.color = color;
        }
        
        public void Dispose()
        {
            GUI.color = guiColor;
        }
    }
}