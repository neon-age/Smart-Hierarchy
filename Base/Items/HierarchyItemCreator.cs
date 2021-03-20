using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal static class HierarchyItemCreator
    {
        private class ItemCreationRules
        {
            public Func<int, HierarchyItem> createForID;
            public Func<Object, HierarchyItem> createForInstance;
        }
        
        private class ItemActivator
        {
            public Func<Object, HierarchyItem> func;
            public Type argumentType;
        }
        
        private static Dictionary<Type, Type> targetsWithItemType = new Dictionary<Type, Type>();
        private static Dictionary<Type, ItemActivator> itemsActivators = new Dictionary<Type, ItemActivator>();
        private static Dictionary<Type, ItemCreationRules> itemsCreationRules = new Dictionary<Type, ItemCreationRules>();
        
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            targetsWithItemType = new Dictionary<Type, Type>();
            
            foreach (var type in TypeCache.GetTypesWithAttribute<HierarchyItemAttribute>())
            {
                if (!typeof(HierarchyItem).IsAssignableFrom(type))
                    continue;
                
                var attribute = type.GetCustomAttribute<HierarchyItemAttribute>();
                var targetType = attribute.targetType;
                var targetTypeMember = attribute.targetTypeMember;

                if (targetTypeMember != null)
                {
                    var typeField = type.GetField(targetTypeMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    targetType = (Type)typeField?.GetValue(null);

                    if (typeField == null)
                    {
                        var typeProperty = type.GetProperty(targetTypeMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        targetType = (Type)typeProperty?.GetValue(null);
                        
                        if (typeProperty == null)
                            Debug.LogError($"Static target type member of name '{targetTypeMember}' was not found in '{type}'.");
                    }
                }
                
                if (targetType != null)
                    targetsWithItemType.Add(targetType, type);
            }

            GetItemsCreationRules();
        }

        private static void GetItemsCreationRules()
        {
            var itemsTypes = TypeCache.GetTypesDerivedFrom<HierarchyItem>();
            
            foreach (var itemType in itemsTypes)
            {
                var createForID = itemType.GetMethod("CreateForInstance",
                    BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance, 
                    null, new[] { typeof(int) }, null);
                
                var createForInstance = itemType.GetMethod("CreateForInstance",
                    BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance, 
                    null, new[] { typeof(Object) }, null);

                if (createForID == null && createForInstance == null)
                    continue;

                var rules = new ItemCreationRules();

                var uninitializedInstance = FormatterServices.GetUninitializedObject(itemType);
                var uninitializedConst = Expression.Convert(Expression.Constant(uninitializedInstance), itemType);
                
                if (createForID != null)
                {
                    var intParam = Expression.Parameter(typeof(int));
                    var func = Expression.Lambda<Func<int, HierarchyItem>>(Expression.Call(uninitializedConst, createForID, intParam), intParam).Compile();

                    rules.createForID = func;
                }
                if (createForInstance != null)
                {
                    var objParam = Expression.Parameter(typeof(Object));
                    var func = Expression.Lambda<Func<Object, HierarchyItem>>(Expression.Call(uninitializedConst, createForInstance, objParam), objParam).Compile();

                    rules.createForInstance = func;
                }

                itemsCreationRules.Add(itemType, rules);
            }
        }
        
        public static HierarchyItem CreateForInstance(int id, Object instance)
        {
            var instanceExist = instance != null;
            
            foreach (var rules in itemsCreationRules.Values)
            {
                HierarchyItem possibleItem = null;

                if (!instanceExist)
                {
                    possibleItem = rules.createForID?.Invoke(id);
                }
                else
                {
                    possibleItem = rules.createForInstance?.Invoke(instance);
                }

                if (possibleItem != null)
                {
                    return possibleItem;
                }
            }
            
            if (instance is GameObject gameObject)
            {
                var components = gameObject.GetComponents<Component>();

                foreach (var component in components)
                {
                    if (component == null)
                        continue;
                    
                    var componentType = component.GetType();
                    
                    targetsWithItemType.TryGetValue(componentType, out var itemType);

                    if (TryGetItemActivator(itemType, out var activator))
                    {
                        Object target = gameObject;
                        
                        if (typeof(Component).IsAssignableFrom(activator.argumentType))
                            target = component;
                        
                        return activator.func.Invoke(target);
                    }
                }
                
                return new GameObjectItem(gameObject) { targetType = typeof(GameObject) };
            }

            return new ViewItem(id);
        }

        private static bool TryGetItemActivator(Type itemType, out ItemActivator activator)
        {
            activator = null;
            
            if (itemType == null)
                return false;
            
            if (!itemsActivators.TryGetValue(itemType, out activator))
            {
                activator = CreateItemActivator(itemType);
                itemsActivators.Add(itemType, activator);
            }
            
            return activator != null;
        }

        private static ItemActivator CreateItemActivator(Type itemType)
        {
            var constructors = itemType.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            
            ConstructorInfo declaredConstructor = null;
            Type argumentType = null;
            
            foreach (var constructor in constructors)
            {
                var arguments = constructor.GetParameters();

                if (arguments.Length == 0 || arguments.Length > 1)
                    continue;
                
                argumentType = arguments[0].ParameterType;

                if (typeof(Object).IsAssignableFrom(argumentType) || 
                    typeof(Component).IsAssignableFrom(argumentType))
                {
                    declaredConstructor = constructor;
                }
            }

            if (declaredConstructor == null)
            {
                Debug.LogError($"ViewItem of type '{itemType.Name}' has no constructor with a single argument that is assignable to 'UnityEngine.Object'.");
                return null;
            }

            var objParam = Expression.Parameter(typeof(Object));
            var convert = Expression.Convert(objParam, argumentType);

            var func = Expression.Lambda<Func<Object, HierarchyItem>>(Expression.New(declaredConstructor, convert), objParam).Compile();
            
            return new ItemActivator { func = func, argumentType = argumentType };
        }
    }
}
