using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MultishotPullInteraction : MonoBehaviour
{
    [SerializeField] private List<Transform> firstSpawnPoints = new();
    [SerializeField] private List<Transform> spawnPoints = new();
    [SerializeField] private Arrow prefab;
    [SerializeField] private float spawnTime = .33f;
    [SerializeField] private float initialWaitTime = .2f;
    private Coroutine _spawnCoroutine;


    public void OnDrawnBack()
    {
        _spawnCoroutine = StartCoroutine(SpawnArrow());
    }

    public void OnReleased()
    {
        foreach (GameObject arrow in _tempArrows)
            Destroy(arrow);
        if (_spawnCoroutine == null) return;
        StopCoroutine(_spawnCoroutine);
        _spawnCoroutine = null;
    }
    List<GameObject> _tempArrows = new List<GameObject>();
    private IEnumerator SpawnArrow()
    {
        Arrow ourArrow = GetComponent<Arrow>();
        yield return new WaitForSeconds(initialWaitTime);
        var t = spawnTime;
        foreach (var spawnPoint in firstSpawnPoints)
        {
            var arrow = Instantiate(prefab, spawnPoint);
            arrow.playerWeapon = ourArrow.playerWeapon;
        }
        Vector3 localScale = Vector3.zero;
        foreach (var spawnPoint in spawnPoints)
        {
            var arrow = Instantiate(prefab, spawnPoint);
            _tempArrows.Add(arrow.gameObject);
            if (localScale == Vector3.zero) 
                localScale = arrow.transform.localScale;
            arrow.transform.localScale = Vector3.zero;
            arrow.playerWeapon = ourArrow.playerWeapon;
        }
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
            foreach (GameObject arrow in _tempArrows)
            {
                float lerp_t = (spawnTime - t) / spawnTime;
                Vector3 scale = math.lerp(Vector3.zero, localScale, lerp_t);
                arrow.transform.localScale = scale;
            }
        }
        //The arrows have fully grown, clear the list
        _tempArrows.Clear();
        
        _spawnCoroutine = null;
    }
}