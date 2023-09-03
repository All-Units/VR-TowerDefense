using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    public GameObject burnedVFX;

    private Coroutine burnCoroutine = null;
    private HealthController _healthController;

    private void Awake()
    {
        _healthController = GetComponentInParent<HealthController>();
    }

    public void StartBurn()
    {
        if (burnCoroutine == null)
        {
            burnCoroutine = StartCoroutine(BurnEffect());
        }
        else
        {
            burnCountdown = 5f;
        }
    }


    private float burnCountdown = 0;
    private IEnumerator BurnEffect()
    {
        burnCountdown = 5f;
        burnedVFX.SetActive(true);

        while (burnCountdown > 0)
        {
            _healthController.TakeDamage(1);
            var startTime = Time.time;
            yield return new WaitForSeconds(1f);
            burnCountdown -= Time.time - startTime;
        }
        
        burnedVFX.SetActive(false);
        burnCoroutine = null;
    }
}
