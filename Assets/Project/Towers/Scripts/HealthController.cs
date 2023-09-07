using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int _currentHealth = 10;

    public event Action<int> OnTakeDamage;
    public event Action OnDeath;
    
    public UnityEvent<int> onTakeDamage;
    public UnityEvent onDeath;

    [SerializeField] private float deathDelay = 1.4f;
    private Animator _anim;
    private void Start()
    {
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
        //Debug.Log($"Taking Damage! {gameObject.name} {_currentHealth}");

        OnTakeDamage?.Invoke(_currentHealth);
        onTakeDamage?.Invoke(_currentHealth);

        if (_currentHealth <= 0)
        {
            OnDeath?.Invoke();
            onDeath?.Invoke();
        }
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