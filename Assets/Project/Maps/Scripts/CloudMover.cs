using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CloudMover : MonoBehaviour
{
    public Vector3 windDirection = Vector3.forward;
    public List<GameObject> cloudPrefabs;
    public float MaxDistanceFromCenter = 2000f;
    public float CloudHeight = 200f;
    public float InitialCloudPoolSize = 30;

    public Vector2 cloudBunchMinMax = new Vector2(2, 5);
    public Vector2 cloudSpeedMinMax = new Vector2(2, 5);

    Vector3 center;
    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;
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
            _SpawnCloud();
        }
    }
    void _SpawnCloud()
    {
        GameObject cloud = Instantiate(cloudPrefabs.GetRandom(), transform);
        float x = Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter);
        float z = Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter);
        cloud.transform.position = new Vector3(x, CloudHeight, z);
        cloud.transform.localScale *= 20f;
        StartCoroutine(_MoveCloud(cloud));  
    }
    IEnumerator _MoveCloud(GameObject cloud)
    {
        Vector3 pos = cloud.transform.position;
        float speed = Random.Range(cloudSpeedMinMax.x, cloudBunchMinMax.y);
        while (pos.FlatDistance(center) < MaxDistanceFromCenter * 2)
        {
            Vector3 dir = windDirection * speed * Time.deltaTime;
            cloud.transform.Translate(dir);
            yield return null;
        }
        _MoveCloudToStart(cloud);
        StartCoroutine(_MoveCloud(cloud));

    }
    void _MoveCloudToStart(GameObject cloud)
    {
        float x = Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter);
        float z = Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter);
        cloud.transform.position = new Vector3(x, CloudHeight, z);

        Vector3 dir = windDirection * -1000f;
        cloud.transform.Translate(dir);
    }

}
