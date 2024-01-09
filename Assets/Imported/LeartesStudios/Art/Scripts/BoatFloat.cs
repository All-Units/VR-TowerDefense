using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatFloat : MonoBehaviour {
    
    public float amplitude = 0.1f; // dalgalarýn geniþliði
    public float frequency = 0.5f; // dalgalarýn hýzý
    public float verticalSpeed = 0.5f; // y ekseni boyunca hareket hýzý
    public float verticalDistance = 0.1f; // y ekseni boyunca hareket mesafesi
    public float maxTiltAngle = 30f; // maksimum yatma açýsý
    public float tiltSpeed = 0.5f; // yatma hýzý

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private float _randomFrequency;
    private float _randomVerticalSpeed;
    private float _randomVerticalDistance;
    private float _randomTiltAngle;
    private float _randomTiltDirection;
    private float _timeOffset;

    private void Start() {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;

        _randomFrequency = Random.Range(frequency * 0.8f, frequency * 1.2f);
        _randomVerticalSpeed = Random.Range(verticalSpeed * 0.8f, verticalSpeed * 1.2f);
        _randomVerticalDistance = Random.Range(verticalDistance * 0.8f, verticalDistance * 1.2f);
        _randomTiltAngle = Random.Range(maxTiltAngle * 0.8f, maxTiltAngle * 1.2f);
        _randomTiltDirection = Random.Range(-1f, 1f);

        _timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update() {
        float offset = Mathf.Sin(Time.time * _randomFrequency + _timeOffset) * _randomVerticalDistance;
        Vector3 newPosition = _initialPosition + new Vector3(0f, offset, 0f);
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * _randomVerticalSpeed);

        float tiltAngle = Mathf.Sin(Time.time * tiltSpeed) * _randomTiltAngle * _randomTiltDirection;
        Quaternion tiltRotation = Quaternion.Euler(tiltAngle, 0f, tiltAngle);
        Quaternion newRotation = _initialRotation * tiltRotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * 5f);

        // X ve Z doðrularýnda maksimum 30 derece dönme sýnýrlandýrmasý
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        float xAngle = eulerAngles.x;
        float zAngle = eulerAngles.z;
        if (xAngle > 180f) xAngle -= 360f;
        if (zAngle > 180f) zAngle -= 360f;
        xAngle = Mathf.Clamp(xAngle, -30f, 30f);
        zAngle = Mathf.Clamp(zAngle, -30f, 30f);
        transform.rotation = Quaternion.Euler(xAngle, eulerAngles.y, zAngle);
    }
}