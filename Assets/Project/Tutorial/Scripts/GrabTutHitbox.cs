using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTutHitbox : MonoBehaviour
{
    public GrabTutorial tutorial;
    HashSet<Collider> colliders = new HashSet<Collider>();
    private void OnTriggerEnter(Collider other)
    {
        other.transform.parent = transform;
        if (colliders.Contains(other)) return;
        colliders.Add(other);
        tutorial.OnCannonballEnterBox();
    }
}
