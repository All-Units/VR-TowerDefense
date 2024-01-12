using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ClusterController : MonoBehaviour
{
    [SerializeField] private float vertMin, vertMax;
    [SerializeField] private int spawnMin, spawnMax;
    [SerializeField] private float speedMin = 1, speedMax = 5;
    [SerializeField] private Projectile spawnable;


    public void SpawnExtras()
    {
        var amountToSpawn = Random.Range(spawnMin, spawnMax);

        for (int i = 0; i < amountToSpawn; i++)
        {
            SpawnAtRandomDirection();
        }
    }

    private void SpawnAtRandomDirection()
    {
        var randomPointOnUnit = Utilities.RandomPointOnUnitCircle();
        var fireDirection = new Vector3(randomPointOnUnit.x, Random.Range(vertMin, vertMax), randomPointOnUnit.y).normalized;

        var proj = Instantiate(spawnable, transform.position + fireDirection,
            Quaternion.FromToRotation(transform.position, fireDirection));
        proj.speed *= Random.Range(speedMin, speedMax);
        proj.Fire();
    }
}