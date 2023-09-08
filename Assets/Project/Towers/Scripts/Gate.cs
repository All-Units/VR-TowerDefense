using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthController))]
public class Gate : MonoBehaviour, IEnemyTargetable
{
    [SerializeField] private bool isFinalGate = false;
    [SerializeField] private HealthController _controller;
    [SerializeField] private GameObject deathParticles;
    

    private void Awake()
    {
        if (_controller == null)
            _controller = GetComponent<HealthController>();
        if (deathParticles == null)
        {
            var p = GetComponentInChildren<ParticleSystem>();
            if (p)
                deathParticles = p.gameObject;
        }
        _controller.onDeath.AddListener(Die);
    }

    public void Die()
    {
        if (deathParticles)
        {
            deathParticles.SetActive(true);
            deathParticles.transform.parent = null;
            Destroy(deathParticles, 5f);
        }
        
        Destroy(gameObject,.01f);
        
        
        //When the final gate falls, the castle falls
        if (isFinalGate)
        {
            GameStateManager.LoseGame();
        }
    }

    public HealthController GetHealthController()
    {
        return _controller;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
