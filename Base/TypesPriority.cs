using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    [Serializable]
    internal class TypesPriority
    {
        [Serializable]
        public class TypeItem
        {
            private Type type;
            public Type Type => type ?? (type = Type.GetType(assemblyQualifiedName) ?? typeof(void));

            public string assemblyQualifiedName;
            public string fullName;
            public int priority;
            
            public static implicit operator TypeItem(string assemblyQualifiedName) => new TypeItem(assemblyQualifiedName);
            
            public TypeItem(string assemblyQualifiedName)
            {
                this.assemblyQualifiedName = assemblyQualifiedName;
                if (Type != null)
                    fullName = Type.FullName;
            }
        }
        
        [SerializeField] private List<TypeItem> types = new List<TypeItem>
        {
            "UnityEngine.Canvas, UnityEngine.UIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            "UnityEngine.Camera, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            "UnityEngine.Light, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            "UnityEngine.ParticleSystem, UnityEngine.ParticleSystemModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
        };
        
        private Dictionary<Type, TypeItem> lookup = new Dictionary<Type, TypeItem>();

        public void Initialize()
        {
            lookup.Clear();
            
            for (var i = 0; i < types.Count; i++)
            {
                var item = types[i];
                
                item.priority = i;
                
                if (!lookup.ContainsKey(item.Type))
                {
                    lookup.Add(item.Type, item);
                }
            }
        }

        public IEnumerable<Component> SelectPrioritizedComponents(params Component[] components)
        {
            var lastPriority = int.MaxValue;
            
            foreach (var component in components)
            {
                if (component == null)
                    continue;
                if (component.hideFlags == HideFlags.HideInInspector)
                    continue;

                var type = component.GetType();
                
                if (TryGetPriority(type, out var priority))
                {
                    if (priority <= lastPriority)
                    {
                        yield return component;
                        lastPriority = priority;
                    }
                }
            }
        }

        public bool TryGetPriority(Type type, out int priority)
        {
            bool TryGetValue(Type key, out TypeItem value)
            {
                value = null;
                return key != null && lookup.TryGetValue(key, out value);
            }
            
            if (TryGetValue(type, out var item) || TryGetValue(type.BaseType, out item))
            {
                priority = item.priority;
                return true;
            }

            priority = int.MaxValue;
            return false;
        }
    }
}