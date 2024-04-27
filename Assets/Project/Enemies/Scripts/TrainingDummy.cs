using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(CapsuleCollider))]
public class TrainingDummy : Enemy
{
    int lastHealthTotal;

    int startHealth;
    Slider healthSlider;
    private void Awake()
    {
        if (healthController == null)
            healthController = GetComponent<HealthController>();
        healthController.OnTakeDamage += _OnTakeDamage;
        healthSlider = GetComponentInChildren<Slider>();
        healthController.OnDeath += _OnDeath;   
        lastHealthTotal = healthController.CurrentHealth;
        startHealth = healthController.MaxHealth;
        GetComponentInChildren<HealthbarController>().DontDestroyOnDeath = true;
    }
    public Transform particlePos;
    public ParticleSystem _deathParticles;
    [Header("Dummy recovery vars")]
    public Transform modelTransform;
    public float _timeSpentDead = 1f;
    public float _timeToRise = 1f;


    void _OnTakeDamage(int currentHealth)
    {
        int dmg = lastHealthTotal - currentHealth;
        if (dmg == 0) return;
        ImpactText.ImpactTextAt(particlePos.position, dmg.ToString(), ImpactText._ImpactTypes.Damage);
        lastHealthTotal = currentHealth;
    }
    private void Start()
    {
        
    }
    private void Update()
    {
        
    }
    private void OnEnable()
    {
        StartCoroutine(_Recover());
    }

    void _OnDeath()
    {
        StartCoroutine(_Recover());
    }
    IEnumerator _Recover()
    {
        _Hitbox.enabled = false;
        _deathParticles.Play();
        modelTransform.gameObject.SetActive(false);
        healthSlider.gameObject.SetActive(false);
        yield return new WaitForSeconds(_timeSpentDead);
        healthSlider.gameObject.SetActive(true);
        modelTransform.gameObject.SetActive(true);
        float t = 0f;
        Vector3 originalRot = modelTransform.localEulerAngles;

        

        Vector3 startRot = originalRot;
        startRot.x = 90f;
        while (t <= _timeToRise)
        {
            Vector3 rot = Vector3.Slerp(startRot, originalRot, (float)(t / _timeToRise));
            modelTransform.localEulerAngles = rot;
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = ((t / _timeToRise));
            t += Time.deltaTime;
            yield return null;
        }
        //modelTransform.eulerAngles = originalRot;

        healthController.isDead = false;
        healthController.SetCurrentHealth(startHealth);
        _Hitbox.enabled = true;
        lastHealthTotal = startHealth;

    }
}
