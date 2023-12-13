using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RingSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float radiusMax, radiusMin;
    [SerializeField] private float yVariance = 0;
    [SerializeField] private float amountPerSecond;
    private float _cooldown = 0f;

    private void Update()
    {
        _cooldown -= Time.deltaTime;
        
        if (_cooldown <= 0)
        {
            var unit = Utilities.RandomPointOnUnitCircle() * Random.Range(radiusMin, radiusMax);
            var pos = new Vector3(unit.x, Random.Range(-yVariance, yVariance), unit.y) + transform.position;
            Instantiate(prefab, pos, Quaternion.identity);

            _cooldown = 1 / amountPerSecond;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radiusMin);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radiusMax);
    }
}