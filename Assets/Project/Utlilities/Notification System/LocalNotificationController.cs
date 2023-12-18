using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalNotificationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TMP_Text text;

    public void PlayNotification(string message)
    {
        var cam = Camera.main.transform.position;
        transform.LookAt(new Vector3(cam.x, transform.position.y, cam.z));
        text.text = message;
        animator.Play("LocalNotificationEnter");
    }
}
