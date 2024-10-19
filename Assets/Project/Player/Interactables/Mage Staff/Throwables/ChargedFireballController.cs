using Codice.CM.Client.Differences.Graphic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChargedFireballController : MonoBehaviour
{
    [SerializeField] ParticleSystem VFX;
    [Tooltip("How long it takes to charge to the next fireball level")]
    [SerializeField] float _chargeTime = 0.3f;
    [Tooltip("How many charge levels this fireball is allowed to have. Base is 0")]
    [SerializeField] int MaxFireballCharge = 3;
    int _currentFireballCharge = 0;
    //How big the VFX should be at each charge level
    [SerializeField] List<float> vfxSizes = new List<float>() {0.5f, 2f, 4f, 6f };
    [SerializeField] List<VFX_Charge_Struct> vfxChargeLevels;
    /// <summary>
    /// Sets the fireball to a given charge level
    /// </summary>
    /// <param name="level"></param>
    void SetChargeLevel(int level = 0)
    {
        //Do nothing if out of bounds
        if (level >= vfxChargeLevels.Count) return;
        var charge = vfxChargeLevels[level];
        var color = VFX.colorOverLifetime;
        color.color = charge.gradient;
        var main = VFX.main;
        main.startSize = charge.chargeSize;
        projectile.speed = charge.speed;

    }

    [System.Serializable]
    public struct VFX_Charge_Struct
    {
        public ParticleSystem.MinMaxGradient gradient;
        public float chargeSize;
        public float speed;
    }

    [Tooltip("How long we calculate the change in distance for")]
    [SerializeField] float rollingDistanceDeltaTime = 1f;
    // Start is called before the first frame update
    void Start()
    {
        projectile = GetComponent<AOEProjectile>();
        StartCoroutine(_CalculateRollingDistance());
        SetChargeLevel();
    }
    AOEProjectile projectile = null;
    LinkedList<PosTimePair> _positions = new LinkedList<PosTimePair>();
    /// <summary>
    /// Calculates the distance this obj has travelled, adding each frame together, dropping old frames
    /// </summary>
    /// <returns></returns>
    IEnumerator _CalculateRollingDistance()
    {
        while (true)
        {
            PosTimePair posTimePair = new PosTimePair(transform);
            //Always add this frame
            _positions.AddLast(posTimePair);
            //Otherwise, check how long the front has existed for
            //If too long, cull
            float timeSince = Time.time - _positions.First.Value.time;
            while (true)
            {
                //If our list isn't full (time wise), wait another frame and take no other action
                if (timeSince <= rollingDistanceDeltaTime) break;
                _positions.RemoveFirst();
                if (_positions.Count == 0 ) break;
                timeSince = Time.time - _positions.First.Value.time;

            }
            yield return null;
        }
    }

    float _LastTotalDistance = 0f;
    int _LastTotalGetFrame = -1;
    /// <summary>
    /// Gets the total distance this fireball has travelled in the last N seconds
    /// </summary>
    public float GetTotalDistance
    {
        get
        {
            if (_positions.Count == 0 ) return 0f;
            if (_LastTotalGetFrame == Time.frameCount) return _LastTotalDistance;
            float distance = 0f;
            Vector3 _lastPos = Vector3.negativeInfinity;
            foreach (var posTimePair in _positions)
            {
                float delta = Vector3.Distance(posTimePair.pos, _lastPos);
                //Ignore very large values
                //Did it this way bc for some reason checking directly for != Vector3.negativeInfinity didn't work?
                //And these values *SHOULD* be small, very rarely above 1
                if (delta >= 100000000f)
                {
                    _lastPos = posTimePair.pos;
                    continue;
                }
                distance += delta;

                _lastPos = posTimePair.pos;
            }
            _LastTotalGetFrame = Time.frameCount;
            _LastTotalDistance = distance;
            return distance;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ChargedFireballController otherController = other.GetComponent<ChargedFireballController>();
        if (otherController == null) return;
        float dist = GetTotalDistance;
        float otherDist = otherController.GetTotalDistance;
        //We moved less than the thing hitting us, destroy other
        if (dist < otherDist)
        {
            _ChargeFireball(otherController);
            
        }
        
    }
    ParticleSystem.MainModule mainVFX => VFX.main;
    /// <summary>
    /// Adds on charge level to the current fireball
    /// </summary>
    /// <param name="other"></param>
    void _ChargeFireball(ChargedFireballController other)
    {
        //Break if we're already fully charged
        if (_currentFireballCharge >= MaxFireballCharge) return;
        //OR currently charging
        if (_currentChargingRoutine != null) return;
        IEnumerator _FireballCharge()
        {
            float t = 0f;
            //Get the size that we're aiming for
            float targetSize = vfxSizes[_currentFireballCharge + 1];
            int nextCharge = _currentFireballCharge + 1;
            targetSize = vfxChargeLevels[nextCharge].chargeSize;
            //Our current size
            float currentSize = vfxSizes[_currentFireballCharge];
            currentSize = vfxChargeLevels[nextCharge].chargeSize;
            ParticleSystem.MainModule main = mainVFX;
            while (t <= _chargeTime)
            {
                yield return null;
                if (XRPauseMenu.IsPaused) continue;
                t += Time.deltaTime;
                
                float lerp_t = (t / _chargeTime);
                main.startSize = Mathf.Lerp(currentSize, targetSize, lerp_t);
            }
            _currentFireballCharge++;
            string msg = $"Charged to Fireball lvl: {_currentFireballCharge}. Target size of {targetSize}";
            print($"{msg}");
            InventoryManager.AppendDebugText(msg);
            main.startSize = targetSize;
            _currentChargingRoutine = null;
            SetChargeLevel(_currentFireballCharge);
        }
        _currentChargingRoutine = _FireballCharge();
        StartCoroutine(_currentChargingRoutine);

        Destroy(other.gameObject);
    }
    IEnumerator _currentChargingRoutine = null;
    struct PosTimePair
    {
        public float time;
        public Vector3 pos;
        //Creates a PosTimePair at a given position at the given time
        public PosTimePair(Transform t)
        {
            time = Time.time;
            pos = t.position;
        }
    }
}
