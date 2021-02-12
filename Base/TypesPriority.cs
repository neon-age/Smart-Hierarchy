using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    [Serializable]
    internal class TypesPriority : ISerializationCallbackReceiver
    {
        [Serializable]
        public class TypeItem
        {
            private Type type;
            public Type Type => type ?? (type = Type.GetType(assemblyQualifiedName) ?? typeof(void));

            public string assemblyQualifiedName;
            public string fullName;
            public bool isIgnored;
            public int priority;
            
            public static implicit operator TypeItem(string assemblyQualifiedName) => new TypeItem(assemblyQualifiedName);
            
            public TypeItem(string assemblyQualifiedName)
            {
                this.assemblyQualifiedName = assemblyQualifiedName;
                if (Type != null)
                    fullName = Type.FullName;
            }

            public override string ToString()
            {
                return Type.AssemblyQualifiedName;
            }
        }
        
        [SerializeField] private List<TypeItem> types = new List<TypeItem>
        {
            "UnityEngine.Canvas, UnityEngine.UIModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            "UnityEngine.Camera, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            "UnityEngine.Light, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            "UnityEngine.ParticleSystem, UnityEngine.ParticleSystemModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            "UnityEngine.Collider, UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            new TypeItem("UnityEngine.MeshRenderer, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") { isIgnored = true },
        };
        
        private Dictionary<Type, TypeItem> lookup = new Dictionary<Type, TypeItem>();

        public void OnBeforeSerialize() {}
        public void OnAfterDeserialize()
        {
            Initialize();
        }

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

        public bool IsIgnored(Component component)
        {
            if (component == null)
                return false;
                
            if (TryGetItem(component.GetType(), out var item))
                return item.isIgnored;
                
            return false;
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
                
                if (TryGetItem(type, out var item))
                {
                    if (item.isIgnored)
                        continue;
                        
                    if (item.priority <= lastPriority)
                    {
                        yield return component;
                        lastPriority = item.priority;
                    }
                }
            }
        }

        public bool TryGetItem(Type type, out TypeItem item)
        {
            bool TryGetValue(Type key, out TypeItem value)
            {
                value = null;
                return key != null && lookup.TryGetValue(key, out value);
            }
            
            return TryGetValue(type, out item) || TryGetValue(type.BaseType, out item);
        }
    }
}