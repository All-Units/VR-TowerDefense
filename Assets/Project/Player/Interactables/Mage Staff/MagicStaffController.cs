using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class MagicStaffController : MonoBehaviour
{
    private Coroutine chargingCoroutine = null;

    [SerializeField] private AudioClipController chargingSFX;
    [SerializeField] private ParticleSystem spellEffect;
    [SerializeField] private ManaModule manaModule;
    [SerializeField] private float manaPerSecond = 1;

    public void BeginSpell()
    {
        if (chargingCoroutine == null)
        {
            if(manaModule.TryUseMana(manaPerSecond * Time.deltaTime))
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

        do
        {
            yield return null;
        } while (manaModule.TryUseMana(manaPerSecond * Time.deltaTime));

        spellEffect.Stop(true);
        chargingCoroutine = null;
    }
}
