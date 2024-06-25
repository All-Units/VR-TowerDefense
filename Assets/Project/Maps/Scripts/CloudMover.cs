using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class CloudMover : MonoBehaviour
{
    public Vector3 windDirection = Vector3.forward;
    public List<GameObject> cloudPrefabs;
    public float MaxDistanceFromCenter = 2000f;
    public float CloudHeight = 200f;
    public float CloudHeightOffset = 20f;
    public float TimeToGrowCloud = 1f;
    public float InitialCloudPoolSize = 30;
    public float SecondaryCloudRate = 0.5f;

    public float RestartScalar = 2f;

    public Vector2 cloudSpeedMinMax = new Vector2(2, 5);
    public Vector2 cloudSizeMinMax = new Vector2(2, 3);
    public float cloudScalar = 4f;


    Vector3 center;
    // Start is called before the first frame update
    void Start()
    {
        center = transform.position;
        _SpawnInitialClouds();
        firstCloudHasReset = false;
        //StartCoroutine(_SpawnSecondaryClouds());
    }
    // Update is called once per frame
    void Update()
    {
    }
    IEnumerator _SpawnSecondaryClouds()
    {
        while (firstCloudHasReset == false)
        {
            yield return new WaitForSeconds(SecondaryCloudRate);
            GameObject cloud = _SpawnCloud(false);
            _MoveCloudToStart(cloud);
        }
    }
    
    void _SpawnInitialClouds()
    {
        for (int i = 0; i < InitialCloudPoolSize; i++)
        {
            _SpawnCloud();
        }
    }
    GameObject _SpawnCloud(bool first = true)
    {
        GameObject cloud = Instantiate(cloudPrefabs.GetRandom(), transform);
        float x = Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter);
        float z = Random.Range(-MaxDistanceFromCenter, MaxDistanceFromCenter);
        Vector3 pos = center + new Vector3(x, CloudHeight, z);
        cloud.transform.position = pos;
        
        if (first == false)
        {
            Vector3 dir = windDirection * -1 * MaxDistanceFromCenter * 1f;

            cloud.transform.Translate(dir);
        }

        StartCoroutine(_MoveCloud(cloud, first));
        return cloud;
    }
    bool firstCloudHasReset = false;
    IEnumerator _MoveCloud(GameObject cloud, bool first = true)
    {
        cloud.transform.localScale = Vector3.one * Random.Range(cloudSizeMinMax.x, cloudSizeMinMax.y);
        float speed = Random.Range(cloudSpeedMinMax.x, cloudSpeedMinMax.y);
        float t = 0f;
        if (first == false)
        {
            
            Vector3 targetScale = cloud.transform.localScale;
            cloud.transform.localScale = Vector3.zero;
            while (t <= TimeToGrowCloud)
            {
                yield return null;
                if (XRPauseMenu.IsPaused)
                {
                    continue;
                }
                t += Time.deltaTime;
                Vector3 scale = Vector3.Slerp(Vector3.zero, targetScale, (t / TimeToGrowCloud));
                cloud.transform.localScale = scale;
                Vector3 dir = windDirection * speed * Time.deltaTime;
                cloud.transform.Translate(dir);

            }
            cloud.transform.localScale = targetScale;
        }
        Vector3 pos = cloud.transform.position;
        //pos.FlatDistance(center) < MaxDistanceFromCenter * 1.2f
        while (cloud.transform.localPosition.x <= MaxDistanceFromCenter)
        {
            if (XRPauseMenu.IsPaused)
            {
                yield return null;
                continue;
            }
            Vector3 dir = windDirection * speed * Time.deltaTime;
            cloud.transform.Translate(dir);
            yield return null;
            pos = cloud.transform.position;
            //cloud.gameObject.name = $"Cloud {cloud.transform.GetSiblingIndex()} - {pos.FlatDistance(center)}m";
        }
        
        //Fade out 
        t = 0f;
        Vector3 currentScale = cloud.transform.localScale;
        while (t <= TimeToGrowCloud)
        {
            yield return null;
            if (XRPauseMenu.IsPaused)
            {
                yield return null;
                continue;
            }
            t += Time.deltaTime;
            
            Vector3 scale = Vector3.Slerp(currentScale, Vector3.zero, (t / TimeToGrowCloud));
            cloud.transform.localScale = scale;
            Vector3 dir = windDirection * speed * Time.deltaTime;
            cloud.transform.Translate(dir);
            
        }
        firstCloudHasReset = true;
        cloud.transform.localScale = currentScale;
        _MoveCloudToStart(cloud, false);
        StartCoroutine(_MoveCloud(cloud, false));
        


    }
    void _MoveCloudToStart(GameObject cloud, bool first = true)
    {
        Vector3 pos = cloud.transform.position;
        
        float height = CloudHeight + Random.Range(-1f * CloudHeightOffset, CloudHeightOffset);
        cloud.transform.position = center + new Vector3(pos.x, height, pos.z);

        Vector3 localpos = cloud.transform.localPosition;
        localpos.x = MaxDistanceFromCenter * -1f * RestartScalar;
        cloud.transform.localPosition = localpos;
    }

}
