using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultishotPullInteraction : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnPoints = new();
    [SerializeField] private Arrow prefab;
    [SerializeField] private float spawnTime = .33f;
    private Coroutine _spawnCoroutine;


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

        foreach (var spawnPoint in spawnPoints)
        {
            Instantiate(prefab, spawnPoint);
        }
        
        _spawnCoroutine = null;
    }
}
