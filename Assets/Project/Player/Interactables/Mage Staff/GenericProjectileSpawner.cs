using UnityEngine;

public class GenericProjectileSpawner : MonoBehaviour
{
    public Projectile projectile;
    public Transform firePoint;

    public void Fire()
    {
        var go = Instantiate(projectile, firePoint.position, firePoint.rotation);
        go.Fire();
        /*Debug.Log(go.name, go);
        Debug.Break();*/
    }

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(firePoint.position, firePoint.position+firePoint.forward);
    }*/
}
