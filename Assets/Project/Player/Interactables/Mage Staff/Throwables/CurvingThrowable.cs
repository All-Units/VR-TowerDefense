using UnityEngine;

public class CurvingThrowable : MonoBehaviour
{
    [SerializeField] private float factor = 1f;
    private Rigidbody _rb;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        _rb.AddForce(Vector3.Cross(_rb.velocity, -_rb.angularVelocity) * (factor * Time.deltaTime), ForceMode.Force);
    }
}
