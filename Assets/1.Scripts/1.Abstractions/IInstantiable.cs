using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public abstract class IInstantiable<T> : MonoBehaviour
    where T : IInstantiable<T>
{
    public static T instance;

    protected virtual void Awake()
    {
        instance = (T)this;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}