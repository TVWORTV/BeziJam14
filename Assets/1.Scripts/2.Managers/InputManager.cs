using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class InputManager : IInstantiable<InputManager>
{

#region Public 

    IWorldObject over = null; 

    public void OnMouseOverObject(
        IWorldObject wobject
    )
    {
        over?.OnMouseExitCallback();
        over = wobject; 
        over?.OnMouseEnterCallback(); 
    }
    public void OnMouseExitObject(
        IWorldObject wobject
    )
    {
        if(wobject == over)
        {
            over?.OnMouseExitCallback();
            over = null; 
        }         
    }

#endregion

#region Loop 

    Vector2 currentPosition;

    void Update()
    {
        currentPosition = GetMouseWorldPosition();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnMouseDown();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            OnMouseUp();
        }

        if (Mouse.current.leftButton.isPressed)
        {
            UpdateMouseDrag();
        }
    }

    void OnMouseDown()
    {
        over?.OnMouseDownCallback();
    }

    void UpdateMouseDrag()
    {

    }

    void OnMouseUp()
    {
        over?.OnMouseUpCallback();
    }

#endregion

#region Helpers 
    
    Vector2 GetMouseWorldPosition()
    {
        Vector3 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        return worldPos;
    }

#endregion

}
