using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimEvents : MonoBehaviour
{
    /// <summary>
    /// Dirty way of calling
    /// </summary>
    public int HasHit;
    private BasicEnemy enemy;
    
    [SerializeField] AudioClipController _ac;

    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponentInParent<BasicEnemy>();
        
    }

    private float lastHit;
    

    public void Impact(float f)
    {
        enemy.Impact();
    }

    public void Footstep()
    {
        _ac.PlayClip();
    }
}
