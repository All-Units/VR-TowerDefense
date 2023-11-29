using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimEvents : MonoBehaviour
{
    /// <summary>
    /// Dirty way of calling
    /// </summary>
    [HideInInspector]
    public int HasHit;
    [SerializeField]
    private BasicEnemy enemy;
    

    // Start is called before the first frame update
    void Start()
    {
        if (enemy == null)
            enemy = GetComponentInParent<BasicEnemy>();
        
    }

    

    public void Impact(float f)
    {
        enemy.Impact();
    }

    public void Footstep()
    {
        if (enemy)
            enemy.Footstep();
        //_ac.PlayClip();
    }
}
