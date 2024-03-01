using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public bool invert;
    public bool FreezeYAxis = false;
    public static Camera main;
    void Update()
    {
        if(main == null)
            main = Camera.main;
        if(main)
        {
            Vector3 target = main.transform.position;
            Vector3 pos = transform.position;
            if (FreezeYAxis)
                target.y = pos.y;
            var transformPosition = invert ? target - pos : pos - target;
            
            transform.rotation = Quaternion.LookRotation(transformPosition);
        }
    }
}
