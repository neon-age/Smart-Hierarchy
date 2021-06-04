using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
	internal static class Patcher
	{
		private static readonly string HarmonyID = typeof(Patcher).Assembly.GetName().Name;
		
		private static Harmony harmony;
		private static List<PatchBase> patches;
		
		private static FieldInfo currentSkinField;
		
		[InitializeOnLoadMethod]
		//[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void Init()
		{
			AwaitRecompileAndInitialize();
		}
		
		private static async void AwaitRecompileAndInitialize() 
		{
			while (EditorApplication.isCompiling || EditorApplication.isUpdating) 
				await Task.Delay(1);
			
			while (!GUISkinHasLoaded())
				await Task.Delay(1);
			
			ApplyPatches();
		}
		
		private static bool GUISkinHasLoaded()
		{
			if (currentSkinField == null)
				currentSkinField = typeof(GUISkin).GetField("current", BindingFlags.Static | BindingFlags.NonPublic);
			
			var skin = (GUISkin)currentSkinField.GetValue(null);
			
			return skin != null && skin.name != "GameSkin";
		}

		internal static void ApplyPatches()
		{
			if (patches == null)
			{
				patches = new List<PatchBase>();
				
				foreach (var type in TypeCache.GetTypesDerivedFrom(typeof(PatchBase)))
				{
					if (type.IsAbstract) 
						continue;
					
					var instance = Activator.CreateInstance(type) as PatchBase;
					patches.Add(instance);
				}
			}
			
			if (harmony == null)
				harmony = new Harmony(HarmonyID);
			
			foreach (var p in patches)
				p.Apply(harmony);
		}

		internal static void RemovePatches()
		{
			if (harmony == null) 
				return;
			
			foreach (var p in patches)
				p.Remove(harmony);
		}
	}
}