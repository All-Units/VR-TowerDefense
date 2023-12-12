using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public bool invert;
    public static Camera main;
    void Update()
    {
        if(main == null)
            main = Camera.main;
        if(main)
        {
            var transformPosition = invert ? main.transform.position - transform.position : transform.position - main.transform.position;
            transform.rotation = Quaternion.LookRotation(transformPosition);
        }
    }
}
