using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    [InitializeOnLoad]
    internal static class FastUtils
    {
        private static readonly Func<Object, IntPtr> getCachedPtr;
        private static readonly Func<int, bool> doesObjectWithIDExist;

        static FastUtils()
        {
            var getCachedPtrInfo = typeof(Object).GetField("m_CachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);
            var getInstanceIDInfo = typeof(Object).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance);
            var doesObjectWithIDExistInfo = typeof(Object).GetMethod("DoesObjectWithInstanceIDExist", BindingFlags.NonPublic | BindingFlags.Static);

            var objParam = Expression.Parameter(typeof(Object));
            var intParam = Expression.Parameter(typeof(int));
            
            // Can this be made faster?
            getCachedPtr = Expression.Lambda<Func<Object, IntPtr>>(Expression.Field(objParam, getCachedPtrInfo), objParam).Compile();
            doesObjectWithIDExist = Expression.Lambda<Func<int, bool>>(Expression.Call(doesObjectWithIDExistInfo, intParam), intParam).Compile();
        }
        
        /// <summary>
        /// About 15-20% faster than UnityObject != null.<para/>
        /// Thread unsafe. No MonoBehaviour checks.
        /// </summary>
        public static bool IsNativeObjectAlive(Object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (getCachedPtr(obj) != IntPtr.Zero)
                return true;
            return doesObjectWithIDExist(obj.GetHashCode());
        }

        public static IntPtr GetCachedPtr(Object obj)
        {
            return getCachedPtr(obj);
        }
        
        public static bool DoesObjectWithInstanceIDExist(int instanceID)
        {
            return doesObjectWithIDExist(instanceID);
        }
    }
}