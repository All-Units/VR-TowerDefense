using System;
using UnityEngine;

[RequireComponent(typeof(HealthController))]
public abstract class Enemy : MonoBehaviour
{
    public float spawnTime { get; private set; }
    public HealthController healthController { get; private set; }
    public static Action<Enemy> OnDeath;

    // Start is called before the first frame update
    private void Awake()
    {
        healthController = GetComponent<HealthController>();
        healthController.OnDeath += HealthControllerOnOnDeath;
        spawnTime = Time.realtimeSinceStartup;
    }

    private void HealthControllerOnOnDeath()
    {
        OnDeath?.Invoke(this);
    }
}