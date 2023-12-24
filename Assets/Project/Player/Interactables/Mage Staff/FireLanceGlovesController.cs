using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class FireLanceGlovesController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The reference to the action to start the select aiming mode for this controller.")]
    private InputActionReference createFireballActionReference;

    public int damage = 10;
    
    private bool isCharging = false;
    private Coroutine chargingCoroutine = null;
    private float chargeTime;
    [SerializeField] private float minChargeTime = .4f;
    [SerializeField] private float maxChargeTime = 2f;
    
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float radius = 2;
    [SerializeField] private float length = 30;
    [SerializeField] private LayerMask layerMask;

    [SerializeField] private GameObject chargingVFX;
    [SerializeField] private ParticleSystem spellVFX;
    [SerializeField] private AudioClipController chargingSFX;

    private void Start()
    {
        var createFireballAction = Utilities.GetInputAction(createFireballActionReference);
        if (createFireballAction != null)
        {
            createFireballAction.started += createFireballActionOnStarted;
            createFireballAction.canceled += createFireballActionOnReleased;
        }
    }

    private void createFireballActionOnStarted(InputAction.CallbackContext obj)
    {
        if(isActiveAndEnabled == false) return;

        isCharging = true;
        if (chargingCoroutine == null)
        {
            chargingCoroutine = StartCoroutine(ChargeAttack());
        }
    }

    private void createFireballActionOnReleased(InputAction.CallbackContext obj)
    {
        if(isActiveAndEnabled == false) return;

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
        if (chargingCoroutine == null) return;
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
            
            HapticFeedback(chargeTime/ maxChargeTime, 0.2f);
            UpdateSpellVFX();
            yield return null;
        } while (chargeTime > 0);

        chargingCoroutine = null;
    }
        
    private void UpdateSpellVFX()
    {
        chargingVFX.transform.localScale = Vector3.one * Mathf.Max(0, chargeTime);
    }

    public void Fire()
    {
        spellVFX.Play();
        var _castRay = new Ray(transform.position, transform.forward);

        var sphereCastAll = Physics.SphereCastAll(_castRay, radius, length, layerMask).ToList();

        foreach (var hit in sphereCastAll)
        {
            Debug.Log($"Hit {hit.transform.gameObject}");
            if(hit.transform.TryGetComponent(out HealthController healthController))
                healthController.TakeDamage(damage);
        }
    }
        
    private void HapticFeedback(float amp,float dur)
    {
        var currentController = gameObject.GetComponentInParent<ActionBasedController>();
        currentController.SendHapticImpulse(amp, dur);
    }
}