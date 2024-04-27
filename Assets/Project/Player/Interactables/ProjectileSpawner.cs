using System;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The projectile that's created")]
    Projectile m_ProjectilePrefab = null;
    protected Projectile projectilePrefab => m_ProjectilePrefab;
    [SerializeField]
    [Tooltip("The point that the project is created")]
    Transform m_StartPoint = null;
    protected Transform startPoint => m_StartPoint;
    [SerializeField]
    [Tooltip("The speed at which the projectile is launched")]
    float m_LaunchSpeed = 1.0f;

    public event Action OnFire;

    [SerializeField] protected OverheatModule overheatModule;
    [SerializeField] private GuidedMissileTargeter _targeter;
    private TowerPlayerWeapon _playerWeapon;

    private void Start()
    {
        _playerWeapon = GetComponent<TowerPlayerWeapon>();
    }

    protected GuidedMissileTargeter targeter => _targeter;
    protected int fired = 0;
    public virtual void Fire()
    {
        if(CheckCantFireModules()) return;
        if (XRPauseMenu.IsPaused) return;
        fired = 0;
        var newObject = Instantiate(m_ProjectilePrefab, m_StartPoint.position, m_StartPoint.rotation, null);
        
        newObject.Fire();
        newObject.playerWeapon = _playerWeapon;
        if (_targeter && newObject.TryGetComponent(out GuidedMissileController guidedMissileController))
        {
            guidedMissileController.targeter = _targeter;
            guidedMissileController.index = 0;
        }
        newObject.gameObject.DestroyAfter(15f);
        fired++;
        //Destroy(newObject, 15f);
        OnFire?.Invoke();
        
        if (_targeter && _targeter.targets.Count > 1)
        {
            
            //Target each selected target first
            for (int i = 1; i < _targeter.targets.Count; i++)
            {
                var projectile = Instantiate(m_ProjectilePrefab, m_StartPoint.position, m_StartPoint.rotation, null);
        
                projectile.Fire();
                projectile.playerWeapon = _playerWeapon;
                if (_targeter && projectile.TryGetComponent(out GuidedMissileController guidedMissile))
                {
                    guidedMissile.targeter = _targeter;
                    guidedMissile.index = i;
                }
                Debug.Log($"firing base", projectile.gameObject);
                projectile.gameObject.DestroyAfter(15f);
                fired++;
                //Destroy(projectile, 15f);
            }
            
        }
    }

    protected bool CheckCantFireModules()
    {
        return overheatModule && overheatModule.IsOverheated();
    }
    
}