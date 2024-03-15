using System;
using System.Collections;
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

    private void Awake()
    {
        _healthController = GetComponentInParent<HealthController>();
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
        if (_burnCoroutine == null)
        {
            _burnTime = 0;
            _burnCoroutine = StartCoroutine(BurnEffect(scalar));
        }
        else
        {
            _burnCountdown = 5f;
        }
    }


    private float _burnCountdown = 0;
    private int _burnLevel = 0;
    private float _burnTime = 0;
    public float burnTimeLevelUp = 3.5f;
    private IEnumerator BurnEffect(int scalar = 1)
    {
        _burnCountdown = 5f;
        _burnLevel = 1;
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

        burnedVFX.SetActive(true);

        while (_burnCountdown > 0)
        {
            if (XRPauseMenu.IsPaused) { yield return null; continue; }
            
            var startTime = Time.time;
            yield return new WaitForSeconds(1f);
            Vector3 forward = _healthController.transform.position + _healthController.transform.forward;
            if (_healthController.isDead == false)
                _healthController.TakeDamageFrom(5 * _burnLevel * scalar, forward);
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
    public void ApplyStatus(StatusEffectType effectType, int burnScalar = 1)
    {
        switch (effectType)
        {
            case StatusEffectType.Burn:
                ApplyBurn();
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