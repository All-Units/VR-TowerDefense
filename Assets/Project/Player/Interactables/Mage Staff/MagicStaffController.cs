using System.Collections;
using UnityEngine;

public class MagicStaffController : MonoBehaviour
{
    private Coroutine chargingCoroutine = null;

    [SerializeField] private AudioClipController chargingSFX;
    [SerializeField] private ParticleSystem spellEffect;

    public void BeginSpell()
    {
        if (chargingCoroutine == null)
        {
            chargingCoroutine = StartCoroutine(ChargeAttack());
        }
    }

    public void ReleaseSpell()
    {
        spellEffect.Stop(true);

        if (chargingCoroutine != null)
        {
            StopCoroutine(chargingCoroutine);
            chargingCoroutine = null;
        }
    }

    private void ResetChargingSequence()
    {
        chargingSFX.Stop();
        StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
    }
    
    private IEnumerator ChargeAttack()
    {
        spellEffect.Play(true);

        while (true)
        {
            yield return null;
        }

        chargingCoroutine = null;
    }
}
