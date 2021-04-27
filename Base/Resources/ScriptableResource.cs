
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class ScriptableResource<T> : ScriptableObject where T : ScriptableResource<T>
    {
        public static T Index => index ? index : index = Resources.Load<T>($"Editor/{ObjectNames.NicifyVariableName(typeof(T).Name)}");
        private static T index;
    }
}