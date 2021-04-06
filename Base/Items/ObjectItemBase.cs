using UnityEngine;

namespace AV.Hierarchy
{
    public class ObjectItemBase : HierarchyItemBase
    {
        public ObjectItemBase(Object instance) : base(instance.GetHashCode(), instance)
        {
        }
        
        protected internal override bool IsItemValid()
        {
            return instance.IsAlive();
        }
    }
}