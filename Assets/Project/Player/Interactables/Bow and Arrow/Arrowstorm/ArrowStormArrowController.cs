using System.Collections;
using UnityEngine;

public class ArrowStormArrowController : MonoBehaviour
{
    [SerializeField] private ArrowstormPlacableController prefab;
    [SerializeField] private ParticleSystem chargedParticles;
    [SerializeField] private float spawnTime = .33f;
    private Coroutine _spawnCoroutine;
    private bool isCharged;
    
    
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
        var t = spawnTime;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
        }

        isCharged = true;
        chargedParticles.Play();
            
        _spawnCoroutine = null;
    }

    public void OnHit()
    {
        if(isCharged == false) return;
        
        var storm = Instantiate(prefab, transform.position, Quaternion.identity);
        
        storm.transform.SetParent(null);
    }
}