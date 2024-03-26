using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HealthController))]
public class Gate : MonoBehaviour, IEnemyTargetable
{
    [SerializeField] private bool isFinalGate = false;
    [SerializeField] private HealthController _controller;
    [SerializeField] private GameObject deathParticles;
    public static Transform FrontGate;
    public static Gate instance;

    public static int FrontGateHealth => instance.GetHealthController().CurrentHealth;
    

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

        if (isFinalGate)
        {
            if (FrontGate != null) Destroy(FrontGate.gameObject);
            instance = this;
            FrontGate = new GameObject().transform;
            FrontGate.gameObject.name = "Front Gate Teleport point";
            FrontGate.parent = transform;
            FrontGate.localPosition = Vector3.zero;
            FrontGate.localRotation = Quaternion.identity;
            FrontGate.localEulerAngles += new Vector3(0f, 95f, 0f);
            FrontGate.localPosition += (transform.forward * 5f);
            FrontGate.Translate(new Vector3(0f, 1f, 0f));
        }
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
    public static void SetFrontGateHealth(int health)
    {
        if (instance == null) return;
        instance.GetHealthController().SetCurrentHealth(health);
        HealthbarController hbc = instance.GetComponentInChildren<HealthbarController>();
        hbc.UpdateValue(health);
    }
}
