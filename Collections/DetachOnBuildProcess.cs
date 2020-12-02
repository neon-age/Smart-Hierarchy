using System.Collections;
using System.Collections.Generic;
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

            if (Application.isEditor && preferences.keepFoldersInPlaymode)
                return;
            
            var sceneRoots = scene.GetRootGameObjects();
            foreach (var root in sceneRoots)
            {
                var folders = root.GetComponentsInChildren<Collection>(true);
                
                foreach (var folder in folders)
                {
                    DetachRootChildren(folder.transform);
                }

                foreach (var folder in folders)
                {
                    Object.DestroyImmediate(folder.gameObject);
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