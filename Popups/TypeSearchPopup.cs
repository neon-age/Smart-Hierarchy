using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace AV.Hierarchy
{
    internal class TypeSearchPopup : ScriptableObject, ISearchWindowProvider
    {
        private List<Type> types;
        private Action<Type> onSelect;
        
        public void Initialize(TypeCache.TypeCollection types, Action<Type> onSelect)
        {
            this.types = new List<Type>(types);
            this.onSelect = onSelect;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            
            var groups = new HashSet<string>();
            var count = types.Count;
            var namespaces = new Dictionary<Type, string>(count);
            
            for (int i = types.Count - 1; i >= 0; i--)
            {
                var type = types[i];

                if (!type.IsVisible)
                {
                    types.RemoveAt(i);
                    continue;
                }
                
                var fullName = type.FullName;

                if (string.IsNullOrEmpty(type.Namespace))
                {
                    fullName = $"Player.{type.Name}";
                }

                namespaces.Add(type, fullName);
            }
            
            var sortedTypes = types.OrderBy(x => namespaces[x]).ToList();
            
            foreach (var type in sortedTypes)
            {
                var names = namespaces[type].Split('.');
                var length = names.Length;
                
                var depth = 1;
                var current = names[0];
                
                foreach (var name in names)
                {
                    if (depth > 1)
                        current += $".{name}";
                    
                    if (length <= depth)
                    {
                        var content = new GUIContent(name, ScriptIcons.GetIcon(type.FullName));
                        entries.Add(new SearchTreeEntry(content) { level = length, userData = type });
                    }
                    else
                    {
                        if (!groups.Contains(current))
                        {
                            groups.Add(current);
                            entries.Add(new SearchTreeGroupEntry(new GUIContent(name)) { level = depth });
                        }
                    }
                    
                    depth++;
                }
            }

            entries.Insert(0, new SearchTreeGroupEntry(new GUIContent("Select type...")));
            
            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            onSelect.Invoke(entry.userData as Type);
            return true;
        }
    }
}