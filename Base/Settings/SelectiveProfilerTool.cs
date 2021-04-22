using UnityEngine;

namespace AV.Hierarchy
{
    public class SelectiveProfilerTool : HierarchyTool
    {
        public bool GCAlloc;
        public bool TimeMS;
        
        protected internal override string title => "Selective Profiler";
        protected internal override Texture2D icon => GetEditorIcon("UnityEditor.ProfilerWindow");
    }
}