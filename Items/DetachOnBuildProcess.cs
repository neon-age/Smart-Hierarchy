using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AV.Hierarchy
{
    internal class DetachOnBuildProcess : IProcessSceneWithReport
    {
        public int callbackOrder { get; }
        
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            var preferences = HierarchySettingsProvider.Preferences;
            var isPlaymode = EditorApplication.isPlayingOrWillChangePlaymode;
                
            if (isPlaymode && preferences.keepCollectionsInPlaymode)
                return;
            
            var sceneRoots = scene.GetRootGameObjects();
            foreach (var root in sceneRoots)
            {
                var collections = root.GetComponentsInChildren<Collection>(true);
                
                foreach (var collection in collections)
                {
                    var isSelfActive = collection.gameObject.activeSelf;
                    var keepTransform = collection.keepTransformHierarchy;
                    
                    // Don't detach from *disabled* collection when 'Keep Transform' is unchecked.
                    // We will destroy it with children on the next step.
                    if (isSelfActive && !keepTransform)
                        DetachRootChildren(collection.transform);
                }

                foreach (var collection in collections)
                {
                    var gameObject = collection.gameObject;
                    var keepTransform = collection.keepTransformHierarchy;
                    
                    if (!isPlaymode)
                        Object.DestroyImmediate(collection);
                    
                    if (!keepTransform)
                        Object.DestroyImmediate(gameObject);
                }
            }
        }

        private void DetachRootChildren(Transform transform)
        {
            var roots = new Transform[transform.childCount];
            var siblings = new int[roots.Length];
            var folderSibling = transform.GetSiblingIndex();

            for (int i = 0; i < roots.Length; i++)
            {
                roots[i] = transform.GetChild(i);
                siblings[i] = transform.GetSiblingIndex();
            }

            for (int i = roots.Length - 1; i >= 0; i--)
            {
                roots[i].SetParent(transform.parent);
                roots[i].SetSiblingIndex(siblings[i]);
            }
                    
            transform.SetSiblingIndex(folderSibling);
        }
    }
}