using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorHandle : MonoBehaviour
{
    [SerializeField] float timeToOpen = 2f;

    [SerializeField] GameObject leftHinge;
    [SerializeField] GameObject rightHinge;

    [SerializeField] Transform runeParent;
    
    [SerializeField] float leftOpenAngle;
    [SerializeField] float rightOpenAngle;

    float leftClosedAngle =-1f;
    float rightClosedAngle = -1f;
    

    
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

    [SerializeField] float howFarToOpen = 100f;
    void startOpenDoor(bool isOpening = true){
        if (leftClosedAngle == -1){ 
            leftClosedAngle = leftHinge.transform.eulerAngles.y;
            rightClosedAngle = rightHinge.transform.eulerAngles.y;

            float openAngle = howFarToOpen;
            leftOpenAngle = leftClosedAngle + openAngle;
            rightOpenAngle = rightClosedAngle - openAngle;
        }

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


        currentDoorOpener = openDoorRoutine();
        StartCoroutine(currentDoorOpener);

    }

    public void OnHandleClose(){
        startOpenDoor(false);
        runeParent.gameObject.SetActive(false);
    }

    public void OnHandleOpen(){
        startOpenDoor();

        runeParent.gameObject.SetActive(true);
        foreach (Transform t in runeParent.GetAllDescendants()){
            ParticleSystem p = t.GetComponent<ParticleSystem>();
            if (p){
                p.Play();
            }
        }
    }

}
