using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum StatusEffectType
{
    Burn,
    Burn2,
    Burn3,
    Freeze,
    Poison
}

public class StatusEffectController : MonoBehaviour
{
    public GameObject burnedVFX;
    public GameObject poisonVFX;

    private Coroutine _burnCoroutine = null;
    private Coroutine _poisonCoroutine = null;
    private HealthController _healthController;
    public DamageDealer damageSource { get; set; }

    private void Awake()
    {
        _healthController = GetComponentInParent<HealthController>();
        gameObject.AddComponent<BasicPausable>();
    }
    public void PointUpwards()
    {
        if (_currentPointer != null) return;

        _currentPointer = _PointUpwards();

        StartCoroutine(_currentPointer);
    }
    IEnumerator _currentPointer = null;
    IEnumerator _PointUpwards()
    {
        while (true)
        {
            //yield return null;
            transform.LookAt(Vector3.forward);
            yield return new WaitForFixedUpdate();
        }
    }

    #region Burn Effect

    public void ApplyBurn(int scalar = 1)
    {
        Debug.LogError($"REACHED LEGACY BURN FUNC, DOING NOTHING");
        return;
        if (_burnCoroutine == null)
        {
            _burnTime = 0;
            //_burnCoroutine = StartCoroutine(BurnEffect(scalar));
            _burnCoroutine = StartCoroutine(BurnEffect());
        }
        else
        {
            _burnCountdown = BaseBurnCountdown;
        }
    }


    private float _burnCountdown = 0;
    private int _burnLevel = 0;
    private float _burnTime = 0;
    public float burnTimeLevelUp = 3.5f;
    const float BaseBurnCountdown = 5f;

    public float EffectDuration => BaseBurnCountdown;

    void _SetBurnColor()
    {
        bool first = true;
        foreach (ParticleSystem particles in burnedVFX.GetComponentsInChildren<ParticleSystem>())
        {
            var main = particles.main;
            Color c = BurnColor;
            if (first == false && AltBurnColor != Color.white)
                c = AltBurnColor;

            main.startColor = c;
            first = false;
        }
        Light light = burnedVFX.GetComponentInChildren<Light>();
        if (light != null)
            light.color = BurnColor;
    }
    
    private IEnumerator BurnEffect()
    {
        _burnCountdown = BaseBurnCountdown;
        _burnLevel = 1;
        _SetBurnColor();

        burnedVFX.SetActive(true);

        while (_burnCountdown > 0)
        {
            var startTime = Time.time;
            yield return new WaitForSeconds(1f);
            if (XRPauseMenu.IsPaused) { yield return null; continue; }
            Vector3 forward = _healthController.transform.position + _healthController.transform.forward;
            if (_healthController.isDead == false)
            {
                //(BaseBurnCountdown -  _burnTime) is to countdown 
                _burnLevel = (int)((BaseBurnCountdown -  _burnTime) / BaseBurnCountdown * 5f);
                _burnLevel = math.max(1, _burnLevel);
                int damage = 2 * _burnLevel * _currentBurnScalar;
                _healthController.TakeDamageFrom(damage, forward, damageSource);
            }
            var elapsedTime = Time.time - startTime;
            _burnCountdown -= elapsedTime;
            _burnTime += elapsedTime;
            if (_burnTime >= burnTimeLevelUp)
            {
                
                _burnLevel++;
                burnTimeLevelUp *= 2;
            }
        }
        
        burnedVFX.SetActive(false);
        _burnCoroutine = null;
    }

    #endregion
    
    #region Poison Effect

    public void ApplyPoison(int level = 1)
    {
        _poisonLevel += level;

        _poisonCoroutine ??= StartCoroutine(PoisonEffect());
    }


    private float _poisonCountdown = 0;
    private int _poisonLevel = 0;
    private IEnumerator PoisonEffect()
    {
        _poisonCountdown = 5f;
        poisonVFX.SetActive(true);

        while (_poisonCountdown > 0)
        {
            _healthController.TakeDamage(1 + _poisonLevel);
            var startTime = Time.time;
            yield return new WaitForSeconds(1f);
            var elapsedTime = Time.time - startTime;
            _poisonCountdown -= elapsedTime;

            if (_poisonCountdown <= 0)
            {
                _poisonCountdown = 5;
                _poisonLevel--;
            }
        }
        
        poisonVFX.SetActive(false);
        _poisonCoroutine = null;
    }

    #endregion
    public Color BurnColor = Color.blue;
    public Color AltBurnColor = Color.white;
    //public void SetBurn(StatusEffectType effectType, int burnScalar = 1, Color primaryColor = new Color(), Color alternateColor = new Color())
    /// <summary>
    /// Tries to apply burn effect. Returns false if that fails
    /// </summary>
    /// <param name="mod"></param>
    /// <returns></returns>
    public bool SetBurn(StatusModifier mod)
    {
        HashSet<StatusEffectType> burnEffects = new HashSet<StatusEffectType>()
        { StatusEffectType.Burn, StatusEffectType.Burn2, StatusEffectType.Burn3};
        //Do nothing if we aren't a burn effect
        if (burnEffects.Contains(mod.effectType) == false)
            return false;
        int scalar = _currentBurnScalar;
        //Always resent burn timer / countdown
        _burnCountdown = BaseBurnCountdown;
        _burnTime = 0;

        //If our burn scalar is higher, override data
        if (mod.BurnScalar > _currentBurnScalar)
        {
            BurnColor = mod.BurnColor;
            AltBurnColor = mod.AltBurnColor;
            _currentBurnScalar = mod.BurnScalar;
            
            _SetBurnColor();
            
        }

        //There is not a current burn active, start one
        if (_burnCoroutine == null)
        {
            _burnCoroutine = StartCoroutine(BurnEffect());
        } 
        

        return true;
    }
    int _currentBurnScalar = -1;
    DamageDealer _currentDamageSource = null;
    public void ApplyStatus(StatusEffectType effectType, int burnScalar = 1)
    {
        switch (effectType)
        {
            case StatusEffectType.Burn:
                ApplyBurn(burnScalar);
                break;

            case StatusEffectType.Burn2:
                ApplyBurn(burnScalar);
                break;
            case StatusEffectType.Burn3:
                ApplyBurn(burnScalar);
                break;
            case StatusEffectType.Freeze:
                break;
            case StatusEffectType.Poison:
                ApplyPoison();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null);
        }
    }
}