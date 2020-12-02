using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using static UnityEditor.EditorGUIUtility;

namespace AV.Hierarchy
{
    [CustomEditor(typeof(Collection))]
    internal class CollectionEditor : Editor
    {
        private static class Reflected
        {
            public static Type gameObjectInspectorType;
            // Need this to hide GameObject editor in inspector
            public static FieldInfo hideInspector;
            
            public static PropertyInfo getInspectorTracker;
            public static Func<object[]> getAllInspectorWindows;

            static Reflected()
            {
                gameObjectInspectorType = typeof(Editor).Assembly.GetType("UnityEditor.GameObjectInspector");
                var inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");

                var getAllInspectorsMethod = inspectorWindowType.GetMethod("GetAllInspectorWindows",
                    BindingFlags.NonPublic | BindingFlags.Static);
                getAllInspectorWindows =
                    Expression.Lambda<Func<object[]>>(Expression.Call(null, getAllInspectorsMethod)).Compile();
                getInspectorTracker = inspectorWindowType.GetProperty("tracker", 
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                hideInspector =
                    typeof(Editor).GetField("hideInspector",
                        BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        private static GUIContent folderIsEmpty;

        private SerializedProperty colorTag;
        
        private new Transform target;
        private new Transform[] targets;
        private GameObject[] children;

        [DidReloadScripts]
        private static void DidReloadScripts()
        {
            // Reselect to update folder header
            var instanceIDs = Selection.instanceIDs;
            Selection.instanceIDs = instanceIDs;
        }
        
        private void OnEnable()
        {
            folderIsEmpty = new GUIContent(" Collection is empty.", IconContent("console.infoicon.sml").image);

            colorTag = serializedObject.FindProperty("colorTag");
            
            target = (base.target as Collection).transform;
            targets = new Transform[base.targets.Length];
            for (int i = 0; i < targets.Length; i++)
                targets[i] = (base.targets[i] as Collection).transform;

            children = new GameObject[target.transform.childCount];
            for (int i = 0; i < target.childCount; i++)
            {
                children[i] = target.GetChild(i).gameObject;
            }

            foreach (var target in targets)
            {
                if((target.hideFlags & HideFlags.HideInInspector) == 0)
                    target.hideFlags |= HideFlags.HideInInspector;
            }

            SetGameObjectInspectorHidden(true);
        }

        private void OnDestroy()
        {
            foreach (var target in targets)
            {
                if (target == null)
                    return;
                // Show transform when folder component is removed
                if (!target.TryGetComponent<Collection>(out _))
                    target.hideFlags ^= HideFlags.HideInInspector;
            }
        }
        
        private void SetGameObjectInspectorHidden(bool hide)
        {
            foreach (var inspector in Reflected.getAllInspectorWindows())
            {
                var tracker = Reflected.getInspectorTracker.GetValue(inspector, null) as ActiveEditorTracker;

                for (var i = 1; i < tracker.activeEditors.Length; i++)
                {
                    var editor = tracker.activeEditors[i];
                    
                    if (editor == null)
                        continue;
                    
                    if (!(editor.target is Collection folder))
                        continue;
                    
                    foreach (var gameObjectInspector in Resources.FindObjectsOfTypeAll(Reflected.gameObjectInspectorType))
                    {
                        var goEditor = gameObjectInspector as UnityEditor.Editor;
                        var gameObject = goEditor.target as GameObject;

                        // Hide GameObject inspector only for folders
                        if (folder.gameObject == gameObject)
                        {
                            Reflected.hideInspector.SetValue(gameObjectInspector, hide);
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(colorTag);
            
            GUILayout.Space(5);
            
            foreach (var child in children)
            {
                EditorGUILayout.BeginHorizontal();
                
                EditorGUI.BeginChangeCheck();
                GUILayout.Toggle(child.activeSelf, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                    child.SetActive(!child.activeSelf);
                
                EditorGUILayout.ObjectField(child, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }

            if (children.Length == 0)
            {
                EditorStyles.label.fontSize += 2;
                GUILayout.Label(folderIsEmpty);
                EditorStyles.label.fontSize -= 2;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}