using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
	internal abstract class PatchBase
	{
		protected static Assembly EditorAssembly { get; } = typeof(Editor).Assembly;

		protected class Patch
		{
			public MethodBase original;
			public string prefix;
			public string postfix;
			public string transpiler;
			public string finalizer;

			public Patch(MethodBase original, 
				string prefix = null, string postfix = null,
				string transpiler = null, string finalizer = null)
			{
				this.original = original;
				this.prefix = prefix;
				this.postfix = postfix;
				this.transpiler = transpiler;
				this.finalizer = finalizer;
			}
		}
		
		private bool applied;
		private List<(MethodBase original, MethodInfo patch)> patches;

		// ReSharper disable once EmptyConstructor
		// ReSharper disable once PublicConstructorInAbstractClass
		// default constructor required for activator
		public PatchBase() {}
		
		protected abstract IEnumerable<Patch> GetPatches();

		public void Apply(Harmony harmony)
		{
			if (applied)
				return;
			applied = true;
			
			if (patches == null)
				patches = new List<(MethodBase original, MethodInfo patch)>();
			
			var type = GetType(); 
			foreach (var patch in GetPatches())
			{
				var original = patch.original;
				var prefix = AccessTools.Method(type, patch.prefix);
				var postfix = AccessTools.Method(type, patch.postfix);
				var transpiler = AccessTools.Method(type, patch.transpiler);
				var finalizer = AccessTools.Method(type, patch.finalizer);
				
				var harmonyPatch = harmony.Patch(original,
					prefix != null ? new HarmonyMethod(prefix) : null,
					postfix != null ? new HarmonyMethod(postfix) : null,
					transpiler != null ? new HarmonyMethod(transpiler) : null,
					finalizer != null ? new HarmonyMethod(finalizer) : null
				);
				patches.Add((original, harmonyPatch)); 
			}
		}

		public void Remove(Harmony instance)
		{
			if (!applied) 
				return;
			applied = false;

			foreach (var method in patches)
			{
				var original = method.original;
				var info = Harmony.GetPatchInfo(original);
				
				info.Prefixes.Do(patchInfo => instance.Unpatch(original, patchInfo.PatchMethod));
				info.Postfixes.Do(patchInfo => instance.Unpatch(original, patchInfo.PatchMethod));
				info.Transpilers.Do(patchInfo => instance.Unpatch(original, patchInfo.PatchMethod));
				info.Finalizers.Do(patchInfo => instance.Unpatch(original, patchInfo.PatchMethod));
			}
			patches.Clear();
		}
	}
}