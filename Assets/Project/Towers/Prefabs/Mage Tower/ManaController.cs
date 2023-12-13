using System;
using UnityEngine;

public class ManaController : MonoBehaviour
{
    public float amount = 1f;

    private void Awake()
    {
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{other.gameObject.name} Entered!");
        if (other.TryGetComponent(out ManaModule module))
        {
            module.CollectMana(amount);
            Destroy(gameObject);
        }
    }
}