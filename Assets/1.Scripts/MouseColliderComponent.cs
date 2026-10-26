using System.Collections.Generic;

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MouseCollider : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Events")]
    public UnityEvent[] onClick;
    public UnityEvent[] onEnter;
    public UnityEvent[] onExit;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        InvokeEvents(onClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InvokeEvents(onEnter);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InvokeEvents(onExit);
    }

    void InvokeEvents(UnityEvent[] events)
    {
        if (events == null)
            return;

        for (int i = 0; i < events.Length; i++)
        {
            events[i]?.Invoke();
        }
    }
}
