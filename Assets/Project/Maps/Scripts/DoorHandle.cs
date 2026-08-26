using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandle : MonoBehaviour
{
    [SerializeField] float timeToOpen = 2f;

    [SerializeField] GameObject leftHinge;
    [SerializeField] GameObject rightHinge;
    
    [SerializeField] float leftOpenAngle;
    [SerializeField] float rightOpenAngle;

    float leftClosedAngle;
    float rightClosedAngle;
    // Start is called before the first frame update
    void Start()
    {
        leftClosedAngle = leftHinge.transform.eulerAngles.y;
        rightClosedAngle = rightHinge.transform.eulerAngles.y;
    }

    
    IEnumerator currentDoorOpener = null;

    float leftTarget;
    float rightTarget;

    float leftStart;
    float rightStart;
    IEnumerator openDoorRoutine(){
        float t = 0;
        while (t < timeToOpen){
            float lerpT = t / timeToOpen;
            lerpTransform(leftHinge.transform, leftStart, leftTarget, lerpT);
            lerpTransform(rightHinge.transform, rightStart, rightTarget, lerpT);
            t += Time.deltaTime;
            yield return null;
        }
    }
    void lerpTransform(Transform t, float start, float end, float lerpT){
        var euler = t.eulerAngles;
        euler.y = Mathf.Lerp(start, end, lerpT);
        t.eulerAngles = euler;
    }

    void startOpenDoor(bool isOpening = true){
        if (currentDoorOpener != null){
            StopCoroutine(currentDoorOpener);
        }
        leftTarget = leftOpenAngle;
        rightTarget = rightOpenAngle;

        leftStart = leftClosedAngle;
        rightStart = rightClosedAngle;

        if (isOpening == false){
            leftTarget = leftClosedAngle;
            rightTarget = rightClosedAngle;

            leftStart = leftOpenAngle;
            rightStart = rightOpenAngle;
        }

        print($"Going to {(isOpening ? "Open" : "Closed")}, left target: {leftTarget}, start: {leftStart}, right target: {rightTarget}, start: {rightStart}");

        currentDoorOpener = openDoorRoutine();
        StartCoroutine(currentDoorOpener);

    }

    public void OnHandleClose(){
        startOpenDoor(false);
    }

    public void OnHandleOpen(){
        startOpenDoor();
    }

}
