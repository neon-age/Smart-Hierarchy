using System;
using UnityEngine;

namespace AV.Hierarchy
{
    /// <summary>
    /// Automatically creates item for instances that represent specified type.<para/>
    /// <para/>
    /// Needs to be used on types inherited from <see cref="GameObjectItemBase"/> or <see cref="ViewItemBase"/>.<para/>
    /// Target type constraints - <see cref="HierarchyItemBase"/>, <see cref="Component"/>, <see cref="GameObject"/> or <see cref="UnityEngine.Object"/>.<para/>
    /// <remarks>
    /// Example:<para/>
    /// <para/>
    /// [HierarchyItem(typeof(Header))]<para/>
    /// public class HeaderItem : <see cref="GameObjectItemBase"/><para/>
    /// {<para/>
    ///     public HeaderItem(GameObject instance) : base(instance) {}<para/>
    /// }
    /// <para/>
    /// <para/>
    /// public class Header : <see cref="HierarchyComponent"/> {}
    /// </remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HierarchyItemAttribute : Attribute
    {
        internal readonly Type targetType;
        internal readonly string targetTypeMember;
        
        public HierarchyItemAttribute(Type targetType)
        {
            this.targetType = targetType;
        }
        
        public HierarchyItemAttribute(string targetTypeMember)
        {
            this.targetTypeMember = targetTypeMember;
        }
    }
}