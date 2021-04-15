using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    internal abstract class HarmonyPatchProvider<T> where T : HarmonyPatchProvider<T>, new()
    {
        protected static Assembly EditorAssembly { get; } = typeof(Editor).Assembly;
        
        protected static Harmony harmony { get; } = new Harmony(typeof(T).FullName);

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
            if (methodInfo == null)
                throw new NullReferenceException($"Provided null method for Patching.");
            
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
                throw new NullReferenceException($"Provided invalid method name for Patching: '{methodName}'");
            
            return new HarmonyMethod(method);
        }
    }
}
