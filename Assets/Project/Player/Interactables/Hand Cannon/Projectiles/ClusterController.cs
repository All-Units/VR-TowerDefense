using UnityEngine;

public class ClusterController : MonoBehaviour
{
    [SerializeField] private float vertMin, vertMax;
    [SerializeField] private int spawnMin, spawnMax;
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
        proj.speed *= Random.Range(.001f, .5f);
        proj.Fire();
    }
}
