using System;
using UnityEngine;

namespace AV.Hierarchy
{
    /// <summary>
    /// Automatically creates <see cref="HierarchyItem"/> for instances that represent specified type.<para/>
    /// Target type can be <see cref="Component"/>, <see cref="GameObject"/> or <see cref="UnityEngine.Object"/>.<para/>
    /// <remarks>
    /// Example:<para/>
    /// <para/>
    /// [HierarchyItem(typeof(Header))]<para/>
    /// public class HeaderItem : <see cref="HierarchyItem"/><para/>
    /// {<para/>
    ///     // Must have a single argument that is assignable from <see cref="UnityEngine.Object"/>!<para/>
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