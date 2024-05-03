using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DragonPetting : MonoBehaviour
{
    public GameObject heartParticlePrefab;

    private void OnTriggerEnter(Collider other)
    {
        var direct = other.GetComponent<XRDirectInteractor>();
        Debug.Log($"Hit {other.gameObject}", other.gameObject);
        if (direct == null) return;

        GameObject hearts = Instantiate(heartParticlePrefab, other.transform.position, Quaternion.identity);
        Destroy(hearts, 3f);
    }
}
