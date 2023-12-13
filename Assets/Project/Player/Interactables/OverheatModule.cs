using System;
using UnityEngine;
using UnityEngine.Events;
public class OverheatModule : MonoBehaviour
{
    [SerializeField] private int shotsToOverheat = 3;
    [SerializeField] private float cooldownRate = 1f;
    private Rigidbody _rb;

    private float currentHeat
    {
        get => _heat;
        set
        {
            _heat = value;
            OnHeatChange?.Invoke(_heat/shotsToOverheat);
        }
    }
    private float _heat;

    private bool _isOverheated = false;
    
    public UnityEvent OnOverHeat;
    public UnityEvent OnCoolDown;
    public UnityEvent<float> OnHeatChange;


    #region Unity Interface

    private void Start()
    {
        var projectileSpawner = GetComponent<ProjectileSpawner>();
        if(projectileSpawner)
            projectileSpawner.OnFire += ProjectileSpawnerOnFire;
        
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(currentHeat <= 0.01) return;

        var rate = 1 / cooldownRate * Time.deltaTime * _rb.velocity.magnitude;
        currentHeat = Mathf.Max(0, currentHeat-rate);
        if (_isOverheated && currentHeat < shotsToOverheat / 3f)
        {
            _isOverheated = false;
            OnCoolDown?.Invoke();
        }
    }

    #endregion

    #region Overheat

    public bool IsOverheated()
    {
        return _isOverheated;
    }
        
    public void ProjectileSpawnerOnFire()
    {
        currentHeat += 1;
        
        if (currentHeat >= shotsToOverheat)
        {
            _isOverheated = true;
            OnOverHeat?.Invoke();
        }
    }

    #endregion

}