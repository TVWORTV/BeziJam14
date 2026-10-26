using System;
using System.Collections;
using System.Collections.Generic;




using UnityEngine;

public abstract class PersistentInstantiable<T> : IInstantiable<T>
    where T : PersistentInstantiable<T>
{
    protected override void Awake()
    {
        base.Awake();

        if (instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
