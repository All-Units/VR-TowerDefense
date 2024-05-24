using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class TracerRound : MonoBehaviour
{
    public float DestroyDelay = 5f;
    public void NullParent()
    {
        transform.parent = null;
        Destroy(gameObject, DestroyDelay);
    }
    
}
