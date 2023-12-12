using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private Vector3 rotation;
    
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotation * Time.deltaTime);
    }
}
