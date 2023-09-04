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

    private Coroutine _burnCoroutine = null;
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
            _burnCoroutine = StartCoroutine(BurnEffect());
        }
        else
        {
            _burnCountdown = 5f;
        }
    }


    private float _burnCountdown = 0;
    private IEnumerator BurnEffect()
    {
        _burnCountdown = 5f;
        burnedVFX.SetActive(true);

        while (_burnCountdown > 0)
        {
            _healthController.TakeDamage(1);
            var startTime = Time.time;
            yield return new WaitForSeconds(1f);
            _burnCountdown -= Time.time - startTime;
        }
        
        burnedVFX.SetActive(false);
        _burnCoroutine = null;
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
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null);
        }
    }
}