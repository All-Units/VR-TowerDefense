using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int _currentHealth = 10;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public event Action<int> OnTakeDamage;
    public event Action<int, Vector3> OnTakeDamageFrom;
    public event Action OnDeath;
    
    public UnityEvent<int> onTakeDamage;
    public UnityEvent onDeath;
    
    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamageFrom(int dmg, Vector3 from)
    {
        TakeDamage(dmg);
        OnTakeDamageFrom?.Invoke(_currentHealth, from);
    }


    /// <summary>
    /// Generic take damage. Will Invoke OnTakeDamage(currentHealth) event and if health is >= 0 will invoke OnDeath.
    /// </summary>
    /// <param name="dmg"></param>
    public void TakeDamage(int dmg)
    {
        if(isDead)return;
        
        _currentHealth -= dmg;
        
        OnTakeDamage?.Invoke(_currentHealth);
        onTakeDamage?.Invoke(_currentHealth);

        if (_currentHealth <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
            onDeath?.Invoke();
            isDead = true;
        }
    }
    
    public void SetMaxHealth(int health)
    {
        _currentHealth = health;
        maxHealth = health;
    }
    public bool isDead = false;
}