using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FlamethrowerController : MonoBehaviour
{
    public int Damage = 1;
    [Tooltip("Always added when a flame is started")]
    public float MinimumCharge = 0.3f;
    public float MinTimeBetweenDamage = 0.2f;
    public StatusModifier modifier;

    [SerializeField] ParticleSystem flameParticles;
    [SerializeField] ParticleSystem smokeParticles;
    [SerializeField] List<ParticleSystem> runeParticles = new List<ParticleSystem>();
    ParticleSystem _firstRune => runeParticles.FirstOrDefault();

    float _runeDuration => runeParticles.FirstOrDefault().main.duration;

    [SerializeField] ParticleSystem.MinMaxGradient _ChargingGradient = new ParticleSystem.MinMaxGradient();

    ParticleSystem.MinMaxGradient _defaultGradient = new ParticleSystem.MinMaxGradient();

    XRGrabInteractable grab;
    public void DestroyThis()
    {
        Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.activated.AddListener(_StartFire);
        grab.deactivated.AddListener(_EndFire);
        grab.firstSelectEntered.AddListener(_Spawned);
        grab.lastSelectExited.AddListener(_Dropped);
        _defaultGradient = runeParticles.FirstOrDefault().colorOverLifetime.color;

        ParticleCollisionHandler handler = GetComponentInChildren<ParticleCollisionHandler>();
        handler._minTimeBetweenDamage = MinTimeBetweenDamage;
        handler.damage = Damage;
        handler.statusModifier = modifier;

        XRPauseMenu.OnPause += _OnPause;
    }
    private void OnDestroy()
    {
        XRPauseMenu.OnPause -= _OnPause;
    }
    void _OnPause()
    {
        _EndFire(new DeactivateEventArgs());
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

        _lastFireballTimes[_whichHand] += MinimumCharge;
        //_SetAllRuneTimes(_lastFireballTime);
        flameParticles.Play();
        _StartOverheat();
        _SetAllRunesToGradient(_ChargingGradient, false, _lastFireballTime);
        //print($"Started fire at time: {_lastFireballTime}s / {_runeDuration}s. Actual time {_GetRuneTime}");
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
        float _time = 0f;
        _time = _lastFireballTime;
        //print($"Fire hands started at {_time}");
        while (_time <= _runeDuration)
        {
            yield return null;
            _time += Time.deltaTime;
            _lastFireballTimes[_whichHand] = _time;
            //print($"Fire hands UPDATED TO {_lastFireballTime}. Actual: {_GetRuneTime}");
        }
        //yield return new WaitForSeconds(_runeDuration);
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
    WhichHand _whichHand;
    static Dictionary<WhichHand, float> _lastFireballTimes = new Dictionary<WhichHand, float>()
    { { WhichHand.left, 0f}, { WhichHand.right, 0f} };
    float _lastFireballTime => _lastFireballTimes[_whichHand];
    void _Spawned(SelectEnterEventArgs a)
    {
        var grabber = a.interactorObject.transform.gameObject;
        bool isLeft = InventoryManager.IsObjLeftHand(grabber);
        bool isRight = InventoryManager.IsObjRightHand(grabber);
        if (isLeft)
            _whichHand = WhichHand.left;
        else if (isRight)
            _whichHand = WhichHand.right;

        //_SetAllRuneTimes(_lastFireballTimes[_whichHand]);

    }
    void _Dropped(SelectExitEventArgs a)
    {
        
        DestroyThis();
    }
    public void Cooldown()
    {
        _lastFireballTimes[_whichHand] = 0f;
        _isOverheated = false;
        smokeParticles.Stop();
        _SetAllRunesToGradient(_defaultGradient);
    }

    void _SetAllRunesToGradient(ParticleSystem.MinMaxGradient gradient, bool looping = true, float time = -1f)
    {
        foreach (var rune in runeParticles)
        {
            var color = rune.colorOverLifetime;
            color.color = gradient;

            rune.Stop();
            rune.Clear();
            if (time == -1f)
            {
                
                //
                rune.time = 0f;
                
            }
                
            else
                rune.time = time;
            
            var main = rune.main;
            main.loop = looping;
            rune.Play();
        }
    }
    float _GetRuneTime => _firstRune.time;
    void _SetAllRuneTimes(float time)
    {
        foreach (var rune in runeParticles)
        {
            rune.time = time;
        }
    }
}
