using System.Collections;
using UnityEngine;

public class ArrowstormPlacableController : MonoBehaviour
{
    public ParticleSystem cloudParticleSystem;
    public ParticleSystem arrowParticleSystem;
    public float buildUpTime, rainTime, whinedDownTime;

    private void Start()
    {
        StartCoroutine(HandleStormLifecycle());
    }

    private IEnumerator HandleStormLifecycle()
    {
        yield return new WaitForSeconds(buildUpTime);
        
        StartArrowRain();

        yield return new WaitForSeconds(rainTime);
        
        StopArrowRain();
        StopClouds();

        yield return new WaitForSeconds(whinedDownTime);
        
        Destroy(gameObject);
    }

    private void StartArrowRain()
    {
        arrowParticleSystem.Play();
    }

    private void StopArrowRain()
    {
        arrowParticleSystem.Stop();
    }

    private void StopClouds()
    {
        cloudParticleSystem.Stop();
    }
}