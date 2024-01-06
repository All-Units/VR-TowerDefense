using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMover : MonoBehaviour
{
    public List<GameObject> cloudPrefabs;
    public float MaxDistanceFromCenter = 2000f;
    public float CloudHeight = 200f;
    public float InitialCloudPoolSize = 30;

    public Vector2 cloudBunchMinMax = new Vector2(2, 5);


    // Start is called before the first frame update
    void Start()
    {
        _SpawnInitialClouds();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void _SpawnInitialClouds()
    {
        for (int i = 0; i < InitialCloudPoolSize; i++)
        {
            GameObject cloud = Instantiate(cloudPrefabs.GetRandom(), transform);
            float x = Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter);
            float z = Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter);
            cloud.transform.position = new Vector3(x, CloudHeight, z);
            cloud.transform.localScale *= 20f;
        }
    }
}
