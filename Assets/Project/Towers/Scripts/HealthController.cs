using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HealthController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int _currentHealth = 10;

    public int CurrentHealth => _currentHealth;
    public event Action<int> OnTakeDamage;
    public event Action OnDeath;
    
    public UnityEvent<int> onTakeDamage;
    public UnityEvent onDeath;

    [SerializeField] private float deathDelay = 1.4f;
    [SerializeField] private Slider healthbar;
    private Animator _anim;
    private void Start()
    {
        if (healthbar == null)
            healthbar = GetComponentInChildren<Slider>();
        
        if (healthbar != null)
            healthbar.value = 1f;
        _anim = GetComponentInChildren<Animator>();
        _currentHealth = maxHealth;
        // OnDeath += _onDeath;
    }

    /// <summary>
    /// Generic take damage. Will Invoke OnTakeDamage(currentHealth) event and if health is >= 0 will invoke OnDeath.
    /// </summary>
    /// <param name="dmg"></param>
    public void TakeDamage(int dmg)
    {
        _currentHealth -= dmg;
        if (healthbar != null)
            healthbar.value = ((float)_currentHealth / (float)maxHealth);
        //Debug.Log($"Taking Damage! {gameObject.name} {_currentHealth}");

        OnTakeDamage?.Invoke(_currentHealth);
        onTakeDamage?.Invoke(_currentHealth);

        if (_currentHealth <= 0)
        {
            OnDeath?.Invoke();
            onDeath?.Invoke();
        }
    }
    public void SetHealthbarActive(bool active)
    {
        if (healthbar != null)
            healthbar.gameObject.SetActive(active);
    }

    public bool isDead = false;
    /*void _onDeath()
    {
        if (isDead)
            return;
        isDead = true;
        //Destroy all colliders / RBs
        foreach (var col in GetComponentsInChildren<Collider>())Destroy(col);
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
        {
            try
            {
                Destroy(rb);
            }
            catch{}
        }    
        
        if (_anim)
        {
            _anim.SetTrigger("death");
            Destroy(gameObject, deathDelay);
        }
        else
            Destroy(gameObject, 0.01f);

    }*/
    
}