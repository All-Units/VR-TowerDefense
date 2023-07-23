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
    
    /// <summary>
    /// Does damage to the tower
    /// </summary>
    /// <param name="dmg"></param>
    /// <returns>If the last attack killed the tower</returns>
    public bool TakeDamage(float dmg)
    {
        bool died = false;
        currentHealth -= dmg;
        
        if (currentHealth <= 0)
            died = true;

        return died;
    }
}
