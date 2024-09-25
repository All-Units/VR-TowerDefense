using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class FireballGlovesController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The reference to the action to start the select aiming mode for this controller.")]
    private InputActionReference createFireballActionReference;
        
    public Projectile projectile;
    public Transform firePoint;
    
    private bool isCharging = false;
    private Coroutine chargingCoroutine = null;
    private float chargeTime;
    [SerializeField] private float minChargeTime = .4f;
    [SerializeField] private float maxChargeTime = 2f;

    [SerializeField] private GameObject spellVFX;
    [SerializeField] private AudioClipController chargingSFX;
    [SerializeField] private XRGrabInteractable throwable;
    [SerializeField] private XRBaseInteractor hand;

    public static event Action OnFireballShoot;

    private void Start()
    {
        var createFireballAction = Utilities.GetInputAction(createFireballActionReference);
        if (createFireballAction != null)
        {
                createFireballAction.started += createFireballActionOnStarted;
                createFireballAction.canceled += createFireballActionOnReleased;
        }
        XRPauseMenu.OnPause += DestroyFireballOnPause;
    }

    void DestroyFireballOnPause()
    {
        if (lastFireball != null && hand.firstInteractableSelected != null)
            Destroy(lastFireball);
    }

    private void createFireballActionOnStarted(InputAction.CallbackContext obj)
    {
        if(isActiveAndEnabled == false) return;
        if (XRPauseMenu.IsPaused) return;
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
                // Fire();
                ResetChargingSequence();
        }
        chargingSFX.Stop();
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
                if (XRPauseMenu.IsPaused) break;
                if (isCharging)
                {
                        chargeTime += Time.deltaTime;
                        chargeTime = Mathf.Min(chargeTime, maxChargeTime);
                        if (chargeTime >= minChargeTime && hand.hasSelection == false)
                        {
                                SpawnAndGrabObject();
                        }
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
        lastFireball = null;
        var go = Instantiate(projectile, firePoint.position, firePoint.rotation);
        go.damage = Mathf.FloorToInt(go.damage * chargeTime);
        int dmg = go.damage;
        go.damage = dmg;
        go.Fire();
        
    }

    GameObject lastFireball = null;
    XRGrabInteractable _lastFireball;
    private void SpawnAndGrabObject()
    {
        var go = Instantiate(throwable, firePoint.position, firePoint.rotation);
        go.interactionManager.SelectEnter((IXRSelectInteractor)hand, go);
        lastFireball = go.gameObject;
        if (chargingCoroutine != null)
            StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;
        _lastFireball = go;
        HapticFeedback();
        go.selectExited.AddListener(_OnThrow);

        Projectile projectile = go.GetComponent<Projectile>();
        TowerPlayerWeapon weapon = throwable.GetComponent<TowerPlayerWeapon>();
        if (projectile != null && weapon != null) 
        {
            projectile.playerWeapon = weapon;
        }
        
        
    }
    void _OnThrow(SelectExitEventArgs a)
    {
        OnFireballShoot?.Invoke();
        if (_lastFireball != null)
        {
            _lastFireball.selectExited.RemoveAllListeners();
            _lastFireball = null;
        }
        HapticFeedback(0.85f);
    }
        
    private void HapticFeedback(float time = 0.5f)
    {
            var currentController = hand.transform.gameObject.GetComponentInParent<ActionBasedController>();
            currentController.SendHapticImpulse(1, time);
    }

    public void SetThrowable(XRGrabInteractable grabInteractable)
    {
            throwable = grabInteractable;
    }
}