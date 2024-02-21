using System;
using System.Collections;
using UnityEngine;

public enum StatusEffectType
{
    Burn,
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

    #region Burn Effect

    public void ApplyBurn()
    {
        if (_burnCoroutine == null)
        {
            _burnTime = 0;
            _burnCoroutine = StartCoroutine(BurnEffect());
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
    private IEnumerator BurnEffect()
    {
        _burnCountdown = 5f;
        _burnLevel = 1;
        burnedVFX.SetActive(true);

        while (_burnCountdown > 0)
        {
            if (XRPauseMenu.IsPaused) { yield return null; continue; }
            _healthController.TakeDamage(5 * _burnLevel);
            var startTime = Time.time;
            yield return new WaitForSeconds(1f);
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
    
    public void ApplyStatus(StatusEffectType effectType)
    {
        switch (effectType)
        {
            case StatusEffectType.Burn:
                ApplyBurn();
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