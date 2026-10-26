using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public static class PoolUtility
{
    // Unity Component pools
    private static readonly Dictionary<Component, object> pools = new();

    // Plain C# object pools
    private static readonly Dictionary<Type, object> objectPools = new();

    // =========================================================
    // UNITY COMPONENT POOLS
    // =========================================================

    /// <summary>
    /// Creates a pool for a prefab and records a component type from it.
    /// </summary>
    public static void CreatePool<T>(
        T prefab,
        int defaultCapacity = 10,
        int maxSize = 100
    ) where T : Component
    {
        if (pools.ContainsKey(prefab))
        {
            Debug.LogWarning($"Pool already exists for {prefab.name}");
            return;
        }

        ObjectPool<T> pool = new(
            createFunc: () =>
            {
                T instance = UnityEngine.Object.Instantiate(prefab);
                instance.gameObject.SetActive(false);
                return instance;
            },

            actionOnGet: obj =>
            {
                obj.gameObject.SetActive(true);
            },

            actionOnRelease: obj =>
            {
                obj.gameObject.SetActive(false);
            },

            actionOnDestroy: obj =>
            {
                UnityEngine.Object.Destroy(obj.gameObject);
            },

            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        pools[prefab] = pool;
    }

    /// <summary>
    /// Gets a Component from its prefab pool.
    /// </summary>
    public static T Get<T>(T prefab) where T : Component
    {
        if (!pools.TryGetValue(prefab, out object poolObj))
        {
            Debug.LogError($"No pool exists for {prefab.name}");
            return null;
        }

        ObjectPool<T> pool = (ObjectPool<T>)poolObj;
        return pool.Get();
    }

    /// <summary>
    /// Releases a Component back into its prefab pool.
    /// </summary>
    public static void Release<T>(T prefab, T instance) where T : Component
    {
        if (!pools.TryGetValue(prefab, out object poolObj))
        {
            Debug.LogError($"No pool exists for {prefab.name}");
            return;
        }

        ObjectPool<T> pool = (ObjectPool<T>)poolObj;
        pool.Release(instance);
    }

    // =========================================================
    // PLAIN C# OBJECT POOLS
    // =========================================================

    /// <summary>
    /// Creates a pool for plain C# objects.
    /// </summary>
    public static void CreateObjectPool<T>(
        Func<T> createFunc,
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        int defaultCapacity = 10,
        int maxSize = 100
    ) where T : class
    {
        Type type = typeof(T);

        if (objectPools.ContainsKey(type))
        {
            Debug.LogWarning($"Object pool already exists for {type.Name}");
            return;
        }

        ObjectPool<T> pool = new(
            createFunc: createFunc,
            actionOnGet: actionOnGet,
            actionOnRelease: actionOnRelease,
            actionOnDestroy: actionOnDestroy,
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );

        objectPools[type] = pool;
    }

    /// <summary>
    /// Gets a plain C# object from its pool.
    /// </summary>
    public static T GetObjectFromPool<T>() where T : class
    {
        Type type = typeof(T);

        if (!objectPools.TryGetValue(type, out object poolObj))
        {
            Debug.LogError($"No object pool exists for {type.Name}");
            return null;
        }

        ObjectPool<T> pool = (ObjectPool<T>)poolObj;
        return pool.Get();
    }

    /// <summary>
    /// Releases a plain C# object back into its pool.
    /// </summary>
    public static void ReleaseObjectToPool<T>(T instance) where T : class
    {
        Type type = typeof(T);

        if (!objectPools.TryGetValue(type, out object poolObj))
        {
            Debug.LogError($"No object pool exists for {type.Name}");
            return;
        }

        ObjectPool<T> pool = (ObjectPool<T>)poolObj;
        pool.Release(instance);
    }

    // =========================================================

    public static void Clear()
    {
        pools.Clear();
        objectPools.Clear();
    }
}