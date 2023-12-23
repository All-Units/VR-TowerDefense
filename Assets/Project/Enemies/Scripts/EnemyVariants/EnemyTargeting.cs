using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyTargeting : MonoBehaviour
{
    [SerializeField] Enemy parent;
    SphereCollider sphere;
    public List<GameObject> targets = new List<GameObject>();
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
        targets.Add(other.gameObject);
        //If we don't have this enemy in our target list
        if (other.TryGetComponent(out IEnemyTargetable enemy)
            && parent.TargetsContains(enemy) == false)
        {
            print($"Greg touched {enemy.GetHealthController().gameObject.name}");
            parent.AddTarget(enemy);
            
            //_targets.Add(enemy);
        }
        else
            print($"Greg touched {other.gameObject.name}, NOT HC");
    }
    private void OnTriggerExit(Collider other)
    {
        targets.Remove(other.gameObject);
        if (other.TryGetComponent(out IEnemyTargetable enemy)
            && parent.TargetsContains(enemy))
        {
            parent.RemoveTarget(enemy);
            //_targets.Remove(enemy);
        }
    }
}
