using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InstaGrowFlowers : MonoBehaviour
{
    [SerializeField] PrefabList_SO flowerList;
    [SerializeField] float minDistance = 0.1f;
    [SerializeField] float timeToGrow = 1f;
    HashSet<Vector3> positions = new HashSet<Vector3>();
    void OnParticleCollision(GameObject other)
    {
        List<ParticleCollisionEvent> events = new List<ParticleCollisionEvent>();
        ParticleSystem ps = WateringCan.particles;
        ParticlePhysicsExtensions.GetCollisionEvents(ps, gameObject, events);
        foreach (ParticleCollisionEvent e in events)
        {
            Vector3 pos = e.intersection;
            float distance = distanceToClosest(pos);
            if (positions.Contains(pos) || distance <= minDistance)
                continue;
            GameObject flower = Instantiate(flowerList.GetRandom(), transform);
            StartCoroutine(growFlower(flower.transform));
            flower.transform.position = pos;
            positions.Add(pos);
        }
    }

    IEnumerator growFlower(Transform flower)
    {
        float t = 0f;
        float s = 0.001f;
        Vector3 small = new Vector3(s, s, s);
        Vector3 target = flower.localScale;
        flower.localScale = small;
        flower.localEulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
        float targetTime = Random.Range(timeToGrow, timeToGrow * 2f);
        while (t <= targetTime)
        {
            yield return new WaitForEndOfFrame();
            flower.localScale = Vector3.Lerp(small, target, t / targetTime);
            t += Time.deltaTime;
        }
    }

    float distanceToClosest(Vector3 pos)
    {
        float d = float.MaxValue;
        foreach (Vector3 position in positions)
        {
            float distance = Vector3.Distance(position, pos);
            if (distance < d)
                d = distance;
        }
        return d;
    }


}
