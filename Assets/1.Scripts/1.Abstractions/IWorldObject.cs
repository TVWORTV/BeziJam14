using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class IWorldObject : MonoBehaviour
{
    public virtual void OnMouseOver(){ }
    public virtual void OnMouseExit(){ }

    public virtual void OnMouseEnterCallback(){ }
    public virtual void OnMouseExitCallback(){ }

    public virtual void OnMouseDownCallback(){ }
    public virtual void OnMouseUpCallback(){ }
}
