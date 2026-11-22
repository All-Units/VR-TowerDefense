using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WateringCan : MonoBehaviour
{
    [SerializeField] ParticleSystem waterParticles;
    public static ParticleSystem particles;
    // Start is called before the first frame update
    void Start()
    {
        particles = waterParticles;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
