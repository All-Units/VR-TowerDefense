using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPoints : MonoBehaviour
{
    public static TeleportPoints instance;

    public static Transform Penthouse => instance.penthouse;
    public static Transform FrontOfGate => instance.frontOfGate;
    public Transform penthouse;
    public Transform frontOfGate;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
