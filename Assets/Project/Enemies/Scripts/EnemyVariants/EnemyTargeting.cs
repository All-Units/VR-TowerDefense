using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyTargeting : MonoBehaviour
{
    [SerializeField] Enemy parent;
    SphereCollider sphere;
    private void Awake()
    {
        if (parent == null)
            parent = GetComponentInParent<Enemy>();
        StartCoroutine(_EnsureSphereEnabled());
    }
    IEnumerator _EnsureSphereEnabled()
    {
        sphere = GetComponent<SphereCollider>();
        float t = Time.time;
        while (Time.time - t <= 2f)
        {
            if (sphere.enabled == false) sphere.enabled = true;
            yield return null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //If we don't have this enemy in our target list
        if (other.TryGetComponent(out IEnemyTargetable enemy)
            && parent.TargetsContains(enemy) == false)
        {
            parent.AddTarget(enemy);
            
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IEnemyTargetable enemy)
            && parent.TargetsContains(enemy))
        {
            parent.RemoveTarget(enemy);
        }
    }
}
