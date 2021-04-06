

using UnityEditor;

namespace AV.Hierarchy
{
    [InitializeOnLoad]
    internal static class GUIUtil
    {
        public static readonly bool isProSkin;

        static GUIUtil()
        {
            isProSkin = EditorGUIUtility.isProSkin;
        }
    }
}