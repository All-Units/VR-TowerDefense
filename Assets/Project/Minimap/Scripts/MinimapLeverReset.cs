using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapLeverReset : MonoBehaviour
{
    [SerializeField] private Transform lever;
    [SerializeField] private float resetRate = 1f;

    private float startX;
    // Start is called before the first frame update
    void Start()
    {
        startX = lever.localEulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Reset()
    {
        
        if (_currentResetter != null)
            StopCoroutine(_currentResetter);
        _currentResetter = _LerpReset();
        StartCoroutine(_LerpReset());
    }

    private IEnumerator _currentResetter = null;

    IEnumerator _LerpReset()
    {
        Vector3 euler = lever.localEulerAngles;
        while (euler.x < startX)
        {
            euler.x += (resetRate * Time.deltaTime);
            lever.localEulerAngles = euler;
            yield return null;
            euler = lever.localEulerAngles;
        }
    }
}
