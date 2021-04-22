
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
    [Serializable]
    internal class HierarchyToolsList
    {  
        [SerializeField]
        [SerializeReference]
        public List<HierarchyTool> list = new List<HierarchyTool>();

        public HierarchyTool this[int index] => list[index];
        
        internal Dictionary<Type, HierarchyTool> lookup = new Dictionary<Type, HierarchyTool>();

        private static TypeCache.TypeCollection ToolTypes = TypeCache.GetTypesDerivedFrom<HierarchyTool>();
        
        
        public void Initialize()
        {
            foreach (var toolType in ToolTypes)
            {
                if (list.Any(x => x.GetType() == toolType))
                    continue;
                    
                var instance = Activator.CreateInstance(toolType) as HierarchyTool;
                list.Add(instance);
            }

            list = list.OrderBy(x => x.order).ToList();

            lookup.Clear();
            
            foreach (var tool in list)
                lookup.Add(tool.GetType(), tool);
        }
    }
}