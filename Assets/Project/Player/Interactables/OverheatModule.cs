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

    public float CurrentHeat;
    #region Unity Interface

    private void Start()
    {
        var projectileSpawner = GetComponent<ProjectileSpawner>();
        if(projectileSpawner)
            projectileSpawner.OnFire += ProjectileSpawnerOnFire;
        
        if (_rb == null)
            _rb = GetComponent<Rigidbody>();
    }

    Vector3 lastPos = Vector3.zero;
    const float MINIMUM_COOLDOWN = 1f;
    private void Update()
    {
        if(currentHeat <= 0.01) return;
        float magnitude = Mathf.Max(_rb.velocity.magnitude, MINIMUM_COOLDOWN);
        var rate = 1 / cooldownRate * Time.deltaTime * magnitude;
        //var rate = 1 / cooldownRate * Time.deltaTime * deltaP;
        currentHeat = Mathf.Max(0, currentHeat - rate);
        
        if (_isOverheated && currentHeat < shotsToOverheat / 3f)
        {
            _isOverheated = false;
            OnCoolDown?.Invoke();
        }
        CurrentHeat = currentHeat;
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