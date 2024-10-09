using System;
using UnityEngine;
using UnityEngine.Events;
public class OverheatModule : MonoBehaviour
{
    public int MaxVelocity = 20;
    public int ShotsToOverheat => shotsToOverheat;
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

    [Header("Debug Vars")]
    public float CurrentHeatDebug;
    public float _LastVelocity;
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
    float _overheatedTime = 0f;
    private void Update()
    {
        CurrentHeatDebug = currentHeat;
        if (currentHeat <= 0.01) return;
        float magnitude = Mathf.Max(_rb.velocity.magnitude, MINIMUM_COOLDOWN);
        magnitude = MathF.Min(magnitude, MaxVelocity);
        _LastVelocity = _rb.velocity.magnitude;
        var rate = (cooldownRate * Time.deltaTime * magnitude);
        
        //var rate = 1 / cooldownRate * Time.deltaTime * deltaP;
        currentHeat = Mathf.Max(0, currentHeat - rate);
        if (_isOverheated && currentHeat < shotsToOverheat / 3f)
        {
            _isOverheated = false;
            OnCoolDown?.Invoke();
        }
        CurrentHeatDebug = currentHeat;
    }

    #endregion

    #region Overheat

    public bool IsOverheated()
    {
        return _isOverheated;
    }
    public void FireMultiple(int count)
    {
        for (int i = 0; i < count; i++)
            ProjectileSpawnerOnFire();
    }
        
    public void ProjectileSpawnerOnFire()
    {
        currentHeat += 1;
        
        if (currentHeat >= shotsToOverheat)
        {
            _isOverheated = true;
            OnOverHeat?.Invoke();
            _overheatedTime = Time.time;
        }
    }

    #endregion

}