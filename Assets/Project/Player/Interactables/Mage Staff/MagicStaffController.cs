using System;
using System.Collections;
using UnityEngine;

public class MagicStaffController : MonoBehaviour
{
    public Projectile projectile;
    public Transform firePoint;
    
    private bool isCharging = false;
    private Coroutine chargingCoroutine = null;
    private float chargeTime;
    [SerializeField] private float minChargeTime = .4f;
    [SerializeField] private float maxChargeTime = 2f;

    [SerializeField] private GameObject spellVFX;
    [SerializeField] private AudioClipController chargingSFX;

    public void BeginCharging()
    {
        isCharging = true;
        if (chargingCoroutine == null)
        {
            chargingCoroutine = StartCoroutine(ChargeAttack());
        }
    }

    public void ReleaseCharging()
    {
        isCharging = false;
        if(chargeTime >= minChargeTime)
        {
            Fire();
            ResetChargingSequence();
        }
    }

    private void ResetChargingSequence()
    {
        chargingSFX.Stop();
        chargeTime = 0;
        StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
    }
    
    private IEnumerator ChargeAttack()
    {
        chargingSFX.PlayClip();
        do
        {
            if (isCharging)
            {
                chargeTime += Time.deltaTime;
                chargeTime = Mathf.Min(chargeTime, maxChargeTime);
            }
            else
            {
                chargeTime -= Time.deltaTime;
            }

            UpdateSpellVFX();
            yield return null;
        } while (chargeTime > 0);

        chargingCoroutine = null;
    }

    private void UpdateSpellVFX()
    {
        spellVFX.transform.localScale = Vector3.one * Mathf.Max(0, chargeTime);
    }

    public void Fire()
    {
        var go = Instantiate(projectile, firePoint.position, firePoint.rotation);
        go.damage = Mathf.FloorToInt(go.damage * chargeTime);
    }
}
