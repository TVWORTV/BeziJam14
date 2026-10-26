using System;
using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorEventBridge : MonoBehaviour
{
    [SerializeField, HideInInspector]
    private List<UnityEvent> events = new();

    public void InvokeEvent(int index)
    {
        if (index < 0 || index >= events.Count)
        {
            Debug.LogWarning($"AnimatorEventBridge: Invalid event index {index}", this);
            return;
        }

        events[index]?.Invoke();
    }

    public void InvokeAll()
    {
        for (int i = 0; i < events.Count; i++)
        {
            events[i]?.Invoke();
        }
    }
}
