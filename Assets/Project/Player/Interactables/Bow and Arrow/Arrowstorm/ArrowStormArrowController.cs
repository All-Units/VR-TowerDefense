using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowStormArrowController : MonoBehaviour
{
    [SerializeField] private ArrowstormPlacableController prefab;
    [SerializeField] private ParticleSystem chargedParticles;
    [SerializeField] private float _timeToCharge = 3f;
    private Coroutine _spawnCoroutine;
    private bool isCharged;

    [SerializeField] List<_StormChargePhase> _StormChargePhases = new List<_StormChargePhase>();
    
    
    public void OnDrawnBack()
    {
        _spawnCoroutine = StartCoroutine(SpawnArrow());
    }
    
    public void OnReleased()
    {
        if (_spawnCoroutine == null) return;
            
        StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = null;
    }
    
    private IEnumerator SpawnArrow()
    {
        
        
        var main = chargedParticles.main;
        var emission = chargedParticles.emission;
        foreach (var phase in _StormChargePhases)
        {
            chargedParticles.Clear();
            main.startSize = phase.Size;
            emission.rateOverTime = phase.SpawnRate;
            chargedParticles.Play();
            yield return new WaitForSeconds(main.duration);
        }

        isCharged = true;
        
            
        _spawnCoroutine = null;
    }

    public void OnHit()
    {
        if(isCharged == false) return;
        
        var storm = Instantiate(prefab, transform.position, Quaternion.identity);
        
        storm.transform.SetParent(null);
    }

    
}
[System.Serializable]
public struct _StormChargePhase
{
    public float Size;
    public float SpawnRate;
}