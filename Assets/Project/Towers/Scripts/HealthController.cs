using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int _currentHealth = 10;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public bool isFull => _currentHealth >= maxHealth;

    public event Action<int> OnTakeDamage;
    public event Action<int, Vector3> OnTakeDamageFrom;
    public event Action OnDeath;
    
    public UnityEvent<int> onTakeDamage;
    public UnityEvent onDeath;

    public void _StartBloodImages()
    {
        if (_currentBleeder != null) return;

        _currentBleeder = _BloodImageRoutine();
        StartCoroutine(_currentBleeder);
    }
    public float initialBleedDelay = 0.5f;
    public float imageChangeRate = 0.3f;
    public Image bloodImage;
    public List<Sprite> bloodSprites = new List<Sprite>();
    IEnumerator _currentBleeder = null;
    IEnumerator _BloodImageRoutine()
    {
        yield return new WaitForSeconds(initialBleedDelay);
        bloodImage.gameObject.SetActive(true);
        foreach (var s in bloodSprites)
        {
            bloodImage.sprite = s;
            yield return new WaitForSeconds(imageChangeRate);
        }
        yield return null;
    }

    
    private void Awake()
    {
        _currentHealth = maxHealth;
    }
    /// <summary>
    /// Takes damage from a given world position
    /// </summary>
    /// <param name="dmg"></param>
    /// <param name="from"></param>
    public void TakeDamageFrom(int dmg, Vector3 from, DamageDealer damageDealer = null)
    {
        if(TakeDamage(dmg) && damageDealer != null)
            damageDealer.OnKill(GetComponent<Enemy>());
            
        OnTakeDamageFrom?.Invoke(_currentHealth, from);
    }


    /// <summary>
    /// Generic take damage. Will Invoke OnTakeDamage(currentHealth) event and if health is >= 0 will invoke OnDeath.
    /// returns true if the entity is killed
    /// </summary>
    /// <param name="dmg"></param>
    public bool TakeDamage(int dmg)
    {
        if(isDead)return false;
        
        _currentHealth -= dmg;
        
        OnTakeDamage?.Invoke(_currentHealth);
        onTakeDamage?.Invoke(_currentHealth);
        //Clamp _currentHealth to 0, can't drop below 0 hp
        if (_currentHealth < 0) _currentHealth = 0;
        if (_currentHealth <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
            onDeath?.Invoke();
            isDead = true;
            return true;
        }

        return false;
    }

    public void Heal(int heal)
    {
        _currentHealth = Mathf.Min(_currentHealth + heal, maxHealth);
        OnTakeDamage?.Invoke(_currentHealth);
        onTakeDamage?.Invoke(_currentHealth);
    }

    public void HealPercent(float per)
    {
        Heal((int)(maxHealth * per));
    }
    
    public void SetMaxHealth(int health)
    {
        _currentHealth = health;
        maxHealth = health;
    }
    public void SetCurrentHealth(int health)
    {
        _currentHealth = health;
        foreach (var slider in GetComponentsInChildren<Slider>())
        {
            slider.value = (float)_currentHealth / (float)maxHealth;
        }
    }
    
    public void ManualDeath()
    {
        isDead = true;
        OnDeath?.Invoke();
        onDeath?.Invoke();
        isDead = true;
    }
    
    public bool isDead = false;
}