using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampOscillator : MonoBehaviour
{
    Vector3 startPos;
    public float speed = 2f;
    public float amplitude = 2f;
    public float rotateSpeed = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        pos.y = startPos.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = pos;
        transform.eulerAngles += new Vector3(0f, rotateSpeed * Time.deltaTime, 0f);
        //print($"Set new pos to {pos}, bc sin of {pos.y * speed} = {Mathf.Sin(pos.y * speed)}");
    }
}
