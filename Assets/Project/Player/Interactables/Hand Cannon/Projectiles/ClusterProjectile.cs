using System.Collections;
using UnityEngine;

public class ClusterProjectile : MonoBehaviour
{
    [SerializeField] private Collider collider;

    [SerializeField] private float waitTime = .3f;

    private void Start()
    {
        StartCoroutine(WaitThenTurnOnColliders());
    }

    private IEnumerator WaitThenTurnOnColliders()
    {
        yield return new WaitForSeconds(waitTime);

        collider.enabled = true;
    }
    
}