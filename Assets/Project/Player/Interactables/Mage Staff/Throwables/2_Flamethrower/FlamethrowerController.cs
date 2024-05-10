using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FlamethrowerController : MonoBehaviour
{
    [SerializeField] ParticleSystem flameParticles;
    [SerializeField] ParticleSystem smokeParticles;
    [SerializeField] List<ParticleSystem> runeParticles = new List<ParticleSystem>();

    float _runeDuration => runeParticles.FirstOrDefault().main.duration;

    [SerializeField] ParticleSystem.MinMaxGradient _ChargingGradient = new ParticleSystem.MinMaxGradient();

    ParticleSystem.MinMaxGradient _defaultGradient = new ParticleSystem.MinMaxGradient();

    XRGrabInteractable grab;
    public void DestroyThis()
    {
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.activated.AddListener(_StartFire);
        grab.deactivated.AddListener(_EndFire);
        grab.lastSelectExited.AddListener(_Dropped);
        _defaultGradient = runeParticles.FirstOrDefault().colorOverLifetime.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    ParticleSystem.Particle[] _currentParticles;
    bool _fireHeld = false;

    void _StartFire(ActivateEventArgs a)
    {
        if (_isOverheated) return;
        flameParticles.Play();
        _StartOverheat();
        _SetAllRunesToGradient(_ChargingGradient, false);
    }
    IEnumerator _currentOverheater = null;
    void _StartOverheat()
    {
        _StopOverheat();
        _currentOverheater = _OverheatRoutine();
        StartCoroutine(_currentOverheater);
    }
    void _StopOverheat()
    {
        if (_currentOverheater != null)
            StopCoroutine(_currentOverheater);
    }
    OverheatModule overheat;
    IEnumerator _OverheatRoutine()
    {
        yield return new WaitForSeconds(_runeDuration);
        if (overheat == null)
            overheat = GetComponent<OverheatModule>();
        int shotCount = overheat.ShotsToOverheat + 1;
        overheat.FireMultiple(shotCount);
        yield return null;
        _isOverheated = true;
        flameParticles.Stop();
        smokeParticles.Play();
    }
    bool _isOverheated = false;
    
    void _EndFire(DeactivateEventArgs a)
    {
        _StopOverheat();
        flameParticles.Stop();
        if (_isOverheated) return;
        _SetAllRunesToGradient(_defaultGradient);
        
    }
    void _Dropped(SelectExitEventArgs a)
    {
        DestroyThis();
    }
    public void Cooldown()
    {
        Debug.Log($"Cooled down", smokeParticles.gameObject);
        _isOverheated = false;
        smokeParticles.Stop();
        _SetAllRunesToGradient(_defaultGradient);
    }

    void _SetAllRunesToGradient(ParticleSystem.MinMaxGradient gradient, bool looping = true)
    {
        foreach (var rune in runeParticles)
        {
            var color = rune.colorOverLifetime;
            color.color = gradient;
            
            rune.Stop();
            rune.Clear();
            rune.Play();
            var main = rune.main;
            main.loop = looping;
        }
    }
}
