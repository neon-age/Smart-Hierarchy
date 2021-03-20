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
            if (!Application.isEditor)
                return;
            
            var sceneRoots = scene.GetRootGameObjects();
            foreach (var root in sceneRoots)
            {
                var items = root.GetComponentsInChildren<HierarchyComponent>(true);
                
                foreach (var item in items)
                {
                    if (item.detachChildren)
                        DetachRootChildren(item.transform);
                }

                foreach (var item in items)
                {
                    if (item.detachChildren)
                        Object.DestroyImmediate(item.gameObject);
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