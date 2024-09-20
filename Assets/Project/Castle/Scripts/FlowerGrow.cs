using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerGrow : MonoBehaviour
{
    [SerializeField] Transform flower;
    [SerializeField] float growRate = 2f;
    [SerializeField] float growTime = 0.8f;

    [SerializeField] float maxScale = 13f;

    private void OnParticleCollision(GameObject other)
    {
        StartCoroutine(_Grower());
    }
    bool _isGrowing = false;
    IEnumerator _Grower()
    {
        if (_isGrowing) yield break;
        _isGrowing = true;
        float t = 0f;
        while (t <= growTime) 
        { 
            if (flower.localScale.x >= maxScale || flower.localScale.y >= maxScale)
                    break;
            yield return null;
            float delta = Time.deltaTime;
            t += delta;
            flower.localScale += (Vector3.one * delta * growRate);
        }
        _isGrowing = false;
    }
}
