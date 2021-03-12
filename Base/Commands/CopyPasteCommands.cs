using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AV.Hierarchy
{
    internal static class CopyPasteCommands
    {
        private static Event evt => Event.current;
        private static HierarchyPreferences prefs => HierarchySettingsProvider.Preferences;
        
        private static bool isIgnored => prefs.copyPastePlace == CopyPastePlace.LastSibling;

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            SceneHierarchyHooks.addItemsToGameObjectContextMenu += (menu, o) => ReplaceMenuFunctions(menu);
            #if UNITY_2020_1_OR_NEWER
            SceneHierarchyHooks.addItemsToSubSceneHeaderContextMenu += (menu, o) => ReplaceMenuFunctions(menu);
            #endif

            void ReplaceMenuFunctions(GenericMenu menu)
            {
                var menuItems = GenericMenuUtil.GetItems(menu);

                for (int i = menuItems.Count - 1; i >= 0; i--)
                {
                    var item = menuItems[i];
                    var content = GenericMenuUtil.GetContent(item);

                    if (content.text == "Paste")
                        GenericMenuUtil.SetFunc(item, Paste);
                        
                    if (content.text == "Duplicate")
                        GenericMenuUtil.SetFunc(item, Duplicate);
                }
            }
        }

        internal static void Paste()
        {
            if (isIgnored)
            {
                SmartHierarchy.active.window.hierarchy.PasteGO();
                return;
            }
            
            if (Selection.transforms.Length == 0)
            {
                SmartHierarchy.active.window.hierarchy.PasteGO();
                return;
            }

            var targetSelection = Selection.activeTransform;
            var isTargetExpanded = SmartHierarchy.active.controller.IsExpanded(Selection.activeInstanceID) && 
                                   Selection.activeTransform.childCount > 0;
            
            var alwaysPasteAsChild = prefs.autoPasteAsChild == AutoPasteAsChild.Always;
            var pasteAsChildWhenExpanded = prefs.autoPasteAsChild == AutoPasteAsChild.OnExpandedSelection;

            
            var oldSelection = GetSiblingsPlace(Selection.transforms, out var siblingIndex);
            
            SmartHierarchy.active.window.hierarchy.PasteGO();

            var selectionChanged = !Selection.transforms.SequenceEqual(oldSelection);

            if (alwaysPasteAsChild || pasteAsChildWhenExpanded && isTargetExpanded)
            {
                foreach (var transform in Selection.transforms)
                {
                    transform.SetParent(targetSelection);
                }
                SetSiblingsInPlaceAndFrame(0, Selection.transforms);
                
                SmartHierarchy.active.ReloadView();
                EditorApplication.DirtyHierarchyWindowSorting();
            }
            else if (selectionChanged)
            {
                SetSiblingsInPlaceAndFrame(siblingIndex, Selection.transforms);
            }
        }
        
        internal static void Duplicate()
        {
            if (isIgnored)
            {
                SmartHierarchy.active.window.hierarchy.DuplicateGO();
                return;
            }
            
            GetSiblingsPlace(Selection.transforms, out var siblingIndex);
            
            SmartHierarchy.active.window.hierarchy.DuplicateGO();
            
            SetSiblingsInPlaceAndFrame(siblingIndex, Selection.transforms);
        }
        
        internal static void ExecuteCommands()
        {
            if (evt.type != EventType.ExecuteCommand && evt.type != EventType.ValidateCommand)
                return;

            var execute = evt.type == EventType.ExecuteCommand;

            if (evt.commandName == "Paste")
            {
                if (execute)
                    Paste();
                Use();
            }
            else if (evt.commandName == "Duplicate")
            {
                if (execute)
                    Duplicate();
                Use();
            }
            
            void Use()
            {
                evt?.Use();
                GUIUtility.ExitGUI();
            }
        }
        
        private static IEnumerable<Transform> GetSiblingsPlace(IEnumerable<Transform> transforms, out int siblingIndex)
        {
            var selections = transforms.OrderBy(x => x.transform.GetSiblingIndex()).ToArray();

            siblingIndex = prefs.copyPastePlace == CopyPastePlace.AfterSelection
                ? selections.Last().GetSiblingIndex() + 1
                : selections.First().GetSiblingIndex();
                
            return selections;
        }
        
        private static void SetSiblingsInPlaceAndFrame(int index, IEnumerable<Transform> transforms)
        {
            transforms = OrderSiblingsAndSetInPlace(index, transforms);
            FrameTransforms(transforms);
        }

        private static void FrameTransforms(IEnumerable<Transform> transforms)
        {
            var objectToFrame = prefs.copyPastePlace == CopyPastePlace.AfterSelection
                ? transforms.Reverse().Last()
                : transforms.Last();

            SmartHierarchy.active.window.FrameObject(objectToFrame.GetInstanceID());
            EditorApplication.DirtyHierarchyWindowSorting();
        }
        
        private static IEnumerable<Transform> OrderSiblingsAndSetInPlace(int index, IEnumerable<Transform> transforms)
        {
            transforms = transforms.OrderBy(x => x.transform.GetSiblingIndex()).Reverse();
            
            foreach (var transform in transforms)
            {
                Undo.SetTransformParent(transform, transform.parent, "Set Sibling Index");
                transform.SetSiblingIndex(index);
                yield return transform;
            }
        }
    }
}