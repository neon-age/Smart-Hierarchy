using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class SerializedPropertyUtil
    {
        public static void DrawPropertyChildren(SerializedProperty property, out bool hasVisibleFields, Rect rect = default)
        {
            hasVisibleFields = false;
            var iterator = property.Copy();

            if (rect == default)
                rect = GUILayoutUtility.GetRect(Screen.width, 0);

            var elementRect = rect;

            var enterChildren = iterator.hasVisibleChildren;
            var endProperty = iterator.GetEndProperty();

            var originalIndent = EditorGUI.indentLevel;
            var relativeIndent = originalIndent - iterator.depth;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                hasVisibleFields = true;
                
                elementRect.height = EditorGUI.GetPropertyHeight(iterator, false);
                EditorGUI.indentLevel = iterator.depth + relativeIndent - 1;

                EditorGUI.BeginChangeCheck();
                enterChildren = EditorGUI.PropertyField(elementRect, iterator, enterChildren);
                // Changing child properties (like array size) may invalidate the iterator, so stop now, or we may get errors.
                if (EditorGUI.EndChangeCheck())
                    break;

                elementRect.y += elementRect.height + EditorGUIUtility.standardVerticalSpacing;
                rect.height += elementRect.height;
            }
            GUILayoutUtility.GetRect(rect.width, rect.height);

            EditorGUI.indentLevel = originalIndent;
        }
    }
}