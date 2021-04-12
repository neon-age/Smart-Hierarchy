using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AV.Hierarchy
{
    internal abstract class HarmonyPatchBase<T> where T : HarmonyPatchBase<T>, new()
    {
        protected static Harmony harmony { get; } = new Harmony(nameof(T));

        private static T instance;

        public static void Initialize()
        {
            if (instance != null)
                return;
            instance = new T();
            instance.OnInitialize();
        }

        protected abstract void OnInitialize();


        protected static void Patch(MethodInfo methodInfo, string prefix = null, string postfix = null)
        {
            var prefixMethod = GetMethod(prefix);
            var postfixMethod = GetMethod(postfix);
            
            harmony.Patch(methodInfo, prefix: prefixMethod, postfix: postfixMethod);
        }
        
        protected static HarmonyMethod GetMethod(string methodName)
        {
            if (methodName == null)
                return null;
            
            var method = typeof(T).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            
            if (method == null)
                throw new NullReferenceException($"Provided invalid method name for patching: '{methodName}'");
            
            return new HarmonyMethod(method);
        }
    }
}
