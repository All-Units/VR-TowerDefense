using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    void Update()
    {
        var main = Camera.main;
        if(main)
            transform.LookAt(main.transform.position);
        
    }
}
