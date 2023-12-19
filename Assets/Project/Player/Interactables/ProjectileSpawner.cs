using System;
using System.Collections;
using System.Collections.Generic;
using PlasticGui.WorkspaceWindow;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The projectile that's created")]
    Projectile m_ProjectilePrefab = null;

    [SerializeField]
    [Tooltip("The point that the project is created")]
    Transform m_StartPoint = null;

    [SerializeField]
    [Tooltip("The speed at which the projectile is launched")]
    float m_LaunchSpeed = 1.0f;

    public event Action OnFire;

    [SerializeField] protected OverheatModule overheatModule;

    public virtual void Fire()
    {
        if(CheckCantFireModules()) return;
        
        var newObject = Instantiate(m_ProjectilePrefab, m_StartPoint.position, m_StartPoint.rotation, null);
        
        newObject.Fire();
        
        Destroy(newObject, 15f);
        OnFire?.Invoke();
    }

    protected bool CheckCantFireModules()
    {
        return overheatModule && overheatModule.IsOverheated();
    }
    
}