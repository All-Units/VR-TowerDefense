using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class FlowerPlacer : MonoBehaviour
{
    public List<GameObject> flowers = new List<GameObject>();
    public Transform placementSpot;
    public float scale = 2f;
    private void Awake()
    {
        

    }
    public void SpawnFlower()
    {
        if (placementSpot == null) return;
        GameObject flower = (GameObject)PrefabUtility.InstantiatePrefab(flowers.GetRandom());
        float y = Random.Range(0f, 360f);
        flower.transform.eulerAngles = new Vector3(0f, y, 0f);
        flower.transform.localScale *= scale;
        flower.transform.parent = placementSpot;
        flower.transform.localPosition = Vector3.zero;
        print("Ran in editor!");
        DestroyImmediate(this);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
