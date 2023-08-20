using System;
using Project.Towers.Scripts;
using UnityEngine;
using UnityEngine.UI;

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

    private void OnDestroy()
    {
        TowerSpawnManager._towersByPos.Remove(transform.position);
    }

    public virtual void Selected()
    {
        Debug.LogError($"No Override implemented for Selected() on {gameObject}", gameObject);
    }    
    public virtual void Deselected()
    {
        Debug.LogError($"No Override implemented for Deselected() on {gameObject}", gameObject);
    }
}