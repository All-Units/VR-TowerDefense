using System.Collections;
using UnityEngine;

public class ArrowstormPlacableController : MonoBehaviour, IPausable
{
    public ParticleSystem cloudParticleSystem;
    public ParticleSystem arrowParticleSystem;
    public float buildUpTime, rainTime, whinedDownTime;

    private IPausableComponents _ipComponents = null;
    public IPausableComponents IPComponents
    {
        get
        {
            if (_ipComponents == null) _ipComponents = this.GetPausableComponents();
            return _ipComponents;
        }
    }

    private void Start()
    {
        StartCoroutine(HandleStormLifecycle());
        OnInitPausable();
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
    void OnDestroy()
    {
        OnDestroyPausable();
    }

    public void OnInitPausable()
    {
        this.InitPausable();
    }

    public void OnDestroyPausable()
    {
        this.DestroyPausable();
    }

    public void OnPause()
    {
        this.BaseOnPause();
    }

    public void OnResume()
    {
        this.BaseOnResume();
    }
}