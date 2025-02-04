﻿using System;
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
    const float MINIMUM_COOLDOWN = 1.5f;
    float _overheatedTime = 0f;
    float _lastRate = 0f;
    float _maxVelocity = -1f;
    private void Update()
    {
        string output = $"Current heat: {currentHeat}\nLast Velocity: {_LastVelocity}\nLast Rate: {_lastRate}";
        //InventoryManager.SetDebugText(output);
        //print(output);
        CurrentHeatDebug = currentHeat;
        if (currentHeat <= 0.01) return;

        //Don't cool down if it's been too close since our last shot
        if (Time.time - _lastFireTime <= 0.5f) return;
        float magnitude = Mathf.Max(_rb.velocity.magnitude, MINIMUM_COOLDOWN);
        _maxVelocity = Mathf.Max(_rb.velocity.magnitude, _maxVelocity);
        magnitude = MathF.Min(magnitude, MaxVelocity);
        _LastVelocity = _rb.velocity.magnitude;
        var rate = (cooldownRate * Time.deltaTime * magnitude);
        _lastRate = rate;
        //var rate = 1 / cooldownRate * Time.deltaTime * deltaP;
        currentHeat = Mathf.Max(0, currentHeat - rate);
        if (_isOverheated && currentHeat < 1f)
        {
            _isOverheated = false;
            OnCoolDown?.Invoke();
            _overheatedTime = Time.time - _overheatedTime;
            string s = $"Cooled down in {_overheatedTime}s\n. Max velocity: {_maxVelocity}";
            InventoryManager.SetDebugText(s);
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
    float _lastFireTime = 0f;    
    public void ProjectileSpawnerOnFire()
    {
        currentHeat += 1;
        _lastFireTime = Time.time;
        if (currentHeat >= shotsToOverheat)
        {
            _isOverheated = true;
            OnOverHeat?.Invoke();
            _overheatedTime = Time.time;
            _maxVelocity = float.MinValue;
        }
    }

    #endregion

}