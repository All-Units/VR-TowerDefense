using UnityEngine;

[RequireComponent(typeof(HealthController))]
public abstract class Enemy : MonoBehaviour
{
    public float spawnTime { get; private set; }
    public HealthController healthController { get; private set; }

    // Start is called before the first frame update
    private void Awake()
    {
        healthController = GetComponent<HealthController>();

        spawnTime = Time.realtimeSinceStartup;
    }
}