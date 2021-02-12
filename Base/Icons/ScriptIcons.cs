using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AV.Hierarchy
{
	internal static class ScriptIcons
	{
		private static Regex addSpaceBeforeCapital = new Regex(@"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))");
		private static TypeCache.TypeCollection types;

		private static Dictionary<Type, MonoScript> monoScripts;
		private static Dictionary<string, Texture> icons = new Dictionary<string, Texture>();

		private static Texture scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
		private static Texture defaultAsset = EditorGUIUtility.IconContent("DefaultAsset Icon").image;
		private static Texture scriptableObject = EditorGUIUtility.IconContent("ScriptableObject Icon").image;
		private static Texture2D transparent;


		public static Texture GetIcon(string typeFullName)
		{
			if (icons.TryGetValue(typeFullName, out var icon))
				return icon;
			
			var name = typeFullName.Split('.').Last();

			Texture TryFindIconByName()
			{
				return FindIcon(name) ?? FindIcon(name + " Icon");
			}

			icon = TryFindIconByName();

			if (icon == null)
			{
				name = name.Replace(" ", "");
				icon = TryFindIconByName();
			}

			if (icon == null)
			{
				name = addSpaceBeforeCapital.Replace(name, " $0");
				icon = TryFindIconByName();
			}

			if (icon != null)
			{
				icons.Add(typeFullName, icon);
				return icon;
			}

			icons.Add(typeFullName, transparent);
			return transparent;
		}

		private static Texture FindIcon(string name)
		{
			Texture icon = null;
			Debug.unityLogger.logEnabled = false;
			try
			{
				icon = EditorGUIUtility.IconContent(name)?.image;
			}
			catch
			{
				// ignored
			}

			Debug.unityLogger.logEnabled = true;
			return icon;
		}

		public static void RetrieveFromScriptTypes(TypeCache.TypeCollection types)
		{
			if (transparent == null)
			{
				transparent = new Texture2D(1, 1);
				transparent.SetPixel(1, 1, new Color(1, 1, 1, 0));
				transparent.Apply();
			}
			
			if (monoScripts == null)
			{
				monoScripts = new Dictionary<Type, MonoScript>();
				FindAllMonoScripts();
			}

			foreach (var type in types)
			{
				if (icons.ContainsKey(type.FullName))
					continue;
				
				monoScripts.TryGetValue(type, out var script);

				var icon = EditorGUIUtility.ObjectContent(script, type).image;

				if (icon == defaultAsset)
					icon = scriptableObject;

				if (!icon)
					icon = transparent;

				icons.Add(type.FullName, icon);
			}
		}

		private static void FindAllMonoScripts()
		{
			var guids = AssetDatabase.FindAssets($"t:MonoScript");

			foreach (var guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
				var type = script.GetClass();

				if (type == null || monoScripts.ContainsKey(type))
					continue;

				monoScripts.Add(type, script);
			}
		}
	}
}