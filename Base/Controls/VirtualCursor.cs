using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    /// Virtual cursor interpolates between mouse positions to fill the gaps caused by fast mouse movement.
    internal class VirtualCursor
    {
        public int MaxInterpolationSteps { get; set; } = 50;
        public bool DebugMode { get; set; }

        private Vector2 position;

        public bool Overlaps(Rect rect)
        {
            var evt = Event.current;
            var eventType = evt.type;
            var mousePos = evt.mousePosition;
            var hasHit = false;

            if (rect.Contains(mousePos))
                return true;
            
            if (eventType != EventType.Repaint && eventType != EventType.Layout)
                return false;

            var delta = Mathf.Abs((position - mousePos).magnitude);
            var stepsCount = (int)Mathf.Lerp(1, MaxInterpolationSteps, delta / 500f);
            
            for (int i = 0; i < stepsCount; i++)
            {
                var mouseStep = Vector2.Lerp(position, mousePos, (float)i / stepsCount);

                if (DebugMode && evt.type == EventType.Repaint)
                    EditorGUI.DrawRect(new Rect(mouseStep, new Vector2(3, 3)), Color.red);

                if (rect.Contains(mouseStep))
                    hasHit = true;
            }
            
            if (hasHit)
                return true;

            return false;
        }

        public void UpdateMousePosition()
        {
            var mousePos = Event.current.mousePosition;

            if (mousePos == Vector2.zero)
                return;

            if (Event.current.type == EventType.Repaint)
                position = mousePos;
        }
    }
}
