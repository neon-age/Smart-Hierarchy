using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AV.Hierarchy
{
    internal static class HierarchyItemCreator
    {
        internal static TreeViewItem currentTreeViewItem;
        internal static HierarchyItemBase currentRootItem;
        internal static bool cancelCreation { get; set; }
        
        private class ItemCreationRules
        {
            public ItemData data;
            public HierarchyItemBase uninitializedItem;
            public Func<int, HierarchyItemBase> createForID;
            public Func<Object, HierarchyItemBase> createForInstance;
            public Func<GameObject, Component, HierarchyItemBase> createForComponent;
        }
        
        private class ItemActivator
        {
            public Func<Object, HierarchyItemBase> func;
            public Type argumentType;
        }

        private class ItemData
        {
            public Type type;
            public ItemActivator activator;
            public HierarchyItemOverrides overrides;

            public static implicit operator Type(ItemData data) => data.type;
        }
        
        private static TypeCache.TypeCollection AllItemsTypes;
        
        private static HashSet<ItemData> GameObjectsLinkedItems = new HashSet<ItemData>();
        //private static Dictionary<Type, List<ItemData>> ObjectsLinkedItems = new Dictionary<Type, List<ItemData>>();
        private static Dictionary<Type, List<ItemData>> ComponentsLinkedItems = new Dictionary<Type, List<ItemData>>();
        
        private static Dictionary<Type, ItemActivator> ItemsActivators = new Dictionary<Type, ItemActivator>();
        private static Dictionary<Type, ItemCreationRules> ItemsCreationRules = new Dictionary<Type, ItemCreationRules>();
        private static Dictionary<Type, HierarchyItemOverrides> ItemsOverrides = new Dictionary<Type, HierarchyItemOverrides>();
        
        private const BindingFlags StaticBind = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags DeclaredOnlyBind = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            AllItemsTypes = TypeCache.GetTypesDerivedFrom<HierarchyItemBase>();
            
            GetItemsOverrides();
            GetItemsActivators();
            GetItemsTargets();
            GetItemsCreationRules();
        }

        #region Get Types Info
        private static void GetItemsTargets()
        {
            foreach (var type in TypeCache.GetTypesWithAttribute<HierarchyItemAttribute>())
            {
                if (!typeof(HierarchyItemBase).IsAssignableFrom(type))
                    continue;
                
                var attribute = type.GetCustomAttribute<HierarchyItemAttribute>();
                var targetType = attribute.targetType;
                var targetTypeMember = attribute.targetTypeMember;

                if (targetTypeMember != null)
                {
                    var typeField = type.GetField(targetTypeMember, StaticBind);
                    targetType = (Type)typeField?.GetValue(null);

                    if (typeField == null)
                    {
                        var typeProperty = type.GetProperty(targetTypeMember, StaticBind);
                        targetType = (Type)typeProperty?.GetValue(null);
                        
                        if (typeProperty == null)
                            Debug.LogError($"Static target type member of name '{targetTypeMember}' was not found in '{type}'.");
                    }
                }

                if (targetType != null)
                {
                    var isItemTarget = typeof(HierarchyItemBase).IsAssignableFrom(targetType);
                    var isObjectTarget = typeof(Object).IsAssignableFrom(targetType);

                    if (isItemTarget || isObjectTarget)
                    {
                        var itemData = GetItemData(type);

                        if (targetType == typeof(GameObject))
                        {
                            GameObjectsLinkedItems.Add(itemData);
                        }
                        else if (typeof(Component).IsAssignableFrom(targetType))
                        {
                            if (!ComponentsLinkedItems.ContainsKey(targetType))
                                ComponentsLinkedItems.Add(targetType, new List<ItemData>());

                            ComponentsLinkedItems[targetType].Add(itemData);
                        }
                    }
                }
            }
        }

        private static ItemData GetItemData(Type itemType)
        {
            ItemsActivators.TryGetValue(itemType, out var activator);
            return new ItemData 
            { 
                type = itemType, 
                activator = activator, 
                overrides = ItemsOverrides[itemType] 
            };
        }

        private static void GetItemsOverrides()
        {
            foreach (var itemType in AllItemsTypes)
            {
                var overrides = HierarchyItemOverrides.Nothing;
                
                // Get overridden methods
                overrides |= IsDeclared(itemType, "OnBeforeIcon") ? HierarchyItemOverrides.OnBeforeIcon : 0;
                overrides |= IsDeclared(itemType, "OnBeforeLabel") ? HierarchyItemOverrides.OnBeforeLabel : 0;
                overrides |= IsDeclared(itemType, "OnBeforeBackground") ? HierarchyItemOverrides.OnBeforeBackground : 0;
                overrides |= IsDeclared(itemType, "OnBeforeAdditionalGUI") ? HierarchyItemOverrides.OnBeforeAdditionalGUI : 0;
                
                overrides |= IsDeclared(itemType, "OnDrawBackground") ? HierarchyItemOverrides.OnDrawBackground : 0;
                overrides |= IsDeclared(itemType, "OnDrawIcon")       ? HierarchyItemOverrides.OnDrawIcon : 0;
                overrides |= IsDeclared(itemType, "OnIconClick")      ? HierarchyItemOverrides.OnIconClick : 0;
                
                overrides |= IsDeclared(itemType, "OnItemGUI")        ? HierarchyItemOverrides.OnItemGUI : 0;
                overrides |= IsDeclared(itemType, "OnAdditionalGUI")  ? HierarchyItemOverrides.OnAdditionalGUI : 0;
                
                ItemsOverrides.Add(itemType, overrides);
            }
        }
        static bool IsDeclared(Type type, string method)
        {
            return type.GetMethod(method, DeclaredOnlyBind) != null;
        }
        
        private static void GetItemsActivators()
        {
            foreach (var itemType in AllItemsTypes)
            {
                if (!typeof(ObjectItemBase).IsAssignableFrom(itemType))
                    continue;
                
                var activator = CreateItemActivator(itemType);
                ItemsActivators.Add(itemType, activator);
            }
        }

        private static void GetItemsCreationRules()
        {
            foreach (var itemType in AllItemsTypes)
            {
                // Get declared CreateForInstance rules
                var createForID = itemType.GetMethod("CreateForInstance", DeclaredOnlyBind, 
                    null, new[] { typeof(int) }, null);
                
                var createForInstance = itemType.GetMethod("CreateForInstance", DeclaredOnlyBind,  
                    null, new[] { typeof(Object) }, null);
                
                var createForComponent = itemType.GetMethod("CreateForInstance", DeclaredOnlyBind, 
                    null, new[] { typeof(GameObject), typeof(Component) }, null);
                
                if (createForID == null && 
                    createForInstance == null && 
                    createForComponent == null)
                    continue;

                var rules = new ItemCreationRules
                {
                    // As C# has no 'static virtual' support, we have to call it on uninitialized object.
                    uninitializedItem = FormatterServices.GetUninitializedObject(itemType) as HierarchyItemBase
                };
                var uninitializedConst = Expression.Convert(Expression.Constant(rules.uninitializedItem), itemType);

                if (createForID != null)
                {
                    var param = Expression.Parameter(typeof(int));
                    var func = Expression.Lambda<Func<int, HierarchyItemBase>>(Expression.Call(uninitializedConst, createForID, param), param).Compile();

                    rules.createForID = func;
                }
                if (createForInstance != null)
                {
                    var param = Expression.Parameter(typeof(Object));
                    var func = Expression.Lambda<Func<Object, HierarchyItemBase>>(Expression.Call(uninitializedConst, createForInstance, param), param).Compile();

                    rules.createForInstance = func;
                }
                if (createForComponent != null)
                {
                    var param = new [] { Expression.Parameter(typeof(GameObject)), Expression.Parameter(typeof(Component)) };
                    var func = Expression.Lambda<Func<GameObject, Component, HierarchyItemBase>>(Expression.Call(uninitializedConst, createForComponent, param), param).Compile();

                    rules.createForComponent = func;
                }

                rules.data = GetItemData(itemType);

                ItemsCreationRules.Add(itemType, rules);
            }
        }
        #endregion

        #region Create For Instance
        public static HierarchyItemBase CreateForInstance(int id, Object instance = default, TreeViewItem viewItem = default)
        {
            var instanceExist = instance != null;
            var gameObject = instance as GameObject;

            HierarchyItemBase root = null;
            Component[] components = null;
            
            currentTreeViewItem = viewItem;

            if (gameObject)
            {
                root = new GameObjectItem(gameObject) { targetType = typeof(GameObject) };
                components = gameObject.GetComponents<Component>();
            }
            else
            {
                root = new ViewItemBase(id);
            }
            
            root.overrides = ItemsOverrides[root.GetType()];
            currentRootItem = root;

            // CreateForInstance() overrides..
            foreach (var rules in ItemsCreationRules.Values)
            {
                HierarchyItemBase item = null;
                
                if (!instanceExist)
                {
                    item = rules.createForID?.Invoke(id);
                }
                else
                {
                    if (rules.createForInstance != null)
                        item = rules.createForInstance.Invoke(instance);

                    if (rules.createForComponent != null && gameObject)
                    {
                        foreach (var component in components)
                        {
                            item = rules.createForComponent.Invoke(gameObject, component);
                            
                            if (IsItemCreated(item))
                                break;
                        }
                    }
                }

                if (IsItemCreated(item))
                {
                    AddItemToStack(root, item, rules.data);
                    CreateLinkedItems(root, item);
                }
            }
            
            if (gameObject)
            {
                // [HierarchyItem(typeof(GameObject))]..
                foreach (var data in GameObjectsLinkedItems)
                {
                    var item = CreateItemFor(instance, root, data);
                    CreateLinkedItems(root, item);
                }
                
                foreach (var component in components)
                {
                    if (component == null)
                        continue;
                    
                    var componentType = component.GetType();

                    if (!ComponentsLinkedItems.TryGetValue(componentType, out var itemsData))
                        continue;

                    foreach (var data in itemsData)
                    {
                        var activator = data.activator;
                        
                        // ObjectItem(Object instance) : base(instance)
                        Object target = instance;

                        // ComponentItem(Component component) : base(component)
                        if (typeof(Component).IsAssignableFrom(activator.argumentType))
                            target = component;

                        var item = activator.func.Invoke(target);
                        AddItemToStack(root, item, data);
                    }
                }
            }

            root.stack = root.stack.OrderByDescending(x => x.order).ToList();
            
            currentTreeViewItem = null;
            currentRootItem = null;
            
            return root;
        }

        static bool IsItemCreated(HierarchyItemBase item)
        {
            var created = !cancelCreation && item != null;
            cancelCreation = false;
            return created;
        }

        static void AddItemToStack(HierarchyItemBase root, HierarchyItemBase item, ItemData data)
        {
            item.overrides = data.overrides;
            root.stack.Add(item);
        }

        static HierarchyItemBase CreateItemFor(Object target, HierarchyItemBase root, ItemData data)
        {
            var item = data.activator.func.Invoke(target);
            AddItemToStack(root, item, data);
            return item;
        }

        static void CreateLinkedItems(HierarchyItemBase root, HierarchyItemBase item)
        {
            var type = item.GetType();
                
            if (!ComponentsLinkedItems.TryGetValue(type, out var linkedItemTypes)) 
                return;
                
            // [HierarchyItem(typeof(OtherHierarchyItem))]..
            foreach (var itemType in linkedItemTypes)
            {
                if (itemType == type)
                {
                    Debug.LogError($"HierarchyItem recursion - trying to link {type.Name} creation to itself.");
                    return;
                }

                CreateItemFor(item.instance, root, itemType);
            }
        }
        #endregion

        #region Item Activator
        private static bool TryGetItemActivator(Type itemType, out ItemActivator activator)
        {
            activator = null;
            
            if (itemType == null)
                return false;
            
            if (!ItemsActivators.TryGetValue(itemType, out activator))
            {
                activator = CreateItemActivator(itemType);
                ItemsActivators.Add(itemType, activator);
            }
            
            return activator != null;
        }

        private static ItemActivator CreateItemActivator(Type itemType)
        {
            var constructors = itemType.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
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

            var func = Expression.Lambda<Func<Object, HierarchyItemBase>>(Expression.New(declaredConstructor, convert), objParam).Compile();
            
            return new ItemActivator { func = func, argumentType = argumentType };
        }
        #endregion
    }
}
