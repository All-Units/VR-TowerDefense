using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public float MaxHealth = 10f;

    public float currentHealth = 10f;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = MaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool TakeDamage(float dmg)
    {
        bool died = false;
        currentHealth -= dmg;
        
        if (currentHealth < 0)
            died = true;

        return died;
    }
}
