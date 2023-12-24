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
                        // Fire();
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
                var go = Instantiate(projectile, firePoint.position, firePoint.rotation);
                go.damage = Mathf.FloorToInt(go.damage * chargeTime);
                go.Fire();
        }

        private void SpawnAndGrabObject()
        {
                var go = Instantiate(throwable, firePoint.position, firePoint.rotation);
                go.interactionManager.SelectEnter((IXRSelectInteractor)hand, go);
                
                StopCoroutine(chargingCoroutine);
                chargingCoroutine = null;

                HapticFeedback();
        }
        
        private void HapticFeedback()
        {
                var currentController = hand.transform.gameObject.GetComponentInParent<ActionBasedController>();
                currentController.SendHapticImpulse(1, 0.5f);
        }

        public void SetThrowable(XRGrabInteractable grabInteractable)
        {
                throwable = grabInteractable;
        }
}