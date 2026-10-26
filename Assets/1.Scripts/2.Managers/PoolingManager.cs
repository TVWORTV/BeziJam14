


using System;
using UnityEngine;

public class PoolManager : IInstantiable<PoolManager>
{

#region MonoBehaviour obj

    public void CreatePool<T>(
        T prefab,
        int defaultCapacity = 10,
        int maxSize = 100
    ) where T : Component
    {
        PoolUtility.CreatePool(prefab, defaultCapacity, maxSize);
    }

    public T Get<T>(T prefab) where T : Component
    {
        return PoolUtility.Get(prefab);
    }

    public void Release<T>(T prefab, T instance) where T : Component
    {
        PoolUtility.Release(prefab, instance);
    }

#endregion

#region Plain obj

    public void CreateObjectPool<T>(
        Func<T> createFunc,
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        int defaultCapacity = 10,
        int maxSize = 100
    ) where T : class
    {
        PoolUtility.CreateObjectPool(
            createFunc,
            actionOnGet,
            actionOnRelease,
            actionOnDestroy,
            defaultCapacity,
            maxSize
        );
    }

    public T GetObject<T>() where T : class
    {
        return PoolUtility.GetObjectFromPool<T>();
    }

    public void ReleaseObject<T>(T instance) where T : class
    {
        PoolUtility.ReleaseObjectToPool(instance);
    }

#endregion

#region Clear 

    public void ClearAll()
    {
        PoolUtility.Clear();
    }

    void OnDestroy()
    {
        PoolUtility.Clear();
    }

#endregion

}