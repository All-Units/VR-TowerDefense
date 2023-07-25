using System;
using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class Tower : MonoBehaviour
{
    public HealthController healthController;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        if(healthController == null)
            healthController = GetComponent<HealthController>();
    }
}