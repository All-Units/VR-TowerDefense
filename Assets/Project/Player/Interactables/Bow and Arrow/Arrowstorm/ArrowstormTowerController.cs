using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowstormTowerController : MonoBehaviour
{
    [SerializeField] ParticleSystem _stormCloud;
    private void Awake()
    {
        ProjectileTower pt = GetComponent<ProjectileTower>();
        pt.onTakeover.AddListener(_OnTakeover);
        pt.onRelease.AddListener(_OnRelease);
    }

    void _OnTakeover()
    {

    }
    void _OnRelease()
    {

    }
}
