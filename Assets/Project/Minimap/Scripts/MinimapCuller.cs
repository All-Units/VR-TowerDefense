using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Collider))]
public class MinimapCuller : MonoBehaviour
{
    [SerializeField] [HideInInspector]
    private bool started = false;
    [SerializeField] [HideInInspector]
    private List<GameObject> children = new List<GameObject>();

    private void Awake()
    {
        Init();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (started == false)
        {
            started = true;
            foreach (Transform t in transform)
                children.Add(t.gameObject);
        }
        if (Minimap.ColliderWasCenter(other) == false) return;
        _SetActive(true);
    }

    void _SetActive(bool set)
    {
        foreach (GameObject go in children)
            go.SetActive(set);
    }

    public void Init()
    {
        if (started == false)
        {
            started = true;
            foreach (Transform t in transform)
                children.Add(t.gameObject);
        }
        _SetActive(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (Minimap.ColliderWasCenter(other) == false) return;
        _SetActive(false);
    }

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
