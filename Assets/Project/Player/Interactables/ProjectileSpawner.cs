using System;
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
    [SerializeField] private GuidedMissileTargeter _targeter;

    public virtual void Fire()
    {
        if(CheckCantFireModules()) return;
        
        var newObject = Instantiate(m_ProjectilePrefab, m_StartPoint.position, m_StartPoint.rotation, null);
        
        newObject.Fire();
        if (_targeter && newObject.TryGetComponent(out GuidedMissileController guidedMissileController))
        {
            guidedMissileController.targeter = _targeter;
            guidedMissileController.index = 0;
        }   
        
        Destroy(newObject, 15f);
        OnFire?.Invoke();

        if (_targeter && _targeter.targets.Count > 1)
        {
            for (int i = 1; i < _targeter.targets.Count; i++)
            {
                var projectile = Instantiate(m_ProjectilePrefab, m_StartPoint.position, m_StartPoint.rotation, null);
        
                projectile.Fire();
                if (_targeter && projectile.TryGetComponent(out GuidedMissileController guidedMissile))
                {
                    guidedMissile.targeter = _targeter;
                    guidedMissile.index = i;
                }        
                
                Destroy(projectile, 15f);
            }
        }
    }

    protected bool CheckCantFireModules()
    {
        return overheatModule && overheatModule.IsOverheated();
    }
    
}