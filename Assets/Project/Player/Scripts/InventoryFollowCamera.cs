using UnityEngine;

public class InventoryFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform camera;
    [SerializeField] private float yOffset = -0.3f;
    [SerializeField] private float yTowerOffset = -0.3f;
    [SerializeField] private float rotateOffset = 90f;
    [SerializeField] private float rotateDamping = 0.3f;
    // Start is called before the first frame update
    void Start()
    {
        _currentDampTime = rotateDamping;
    }

    private Vector3 _velocity = Vector3.zero;

    private float _currentDampTime = 0f;

    private Quaternion _target = Quaternion.identity;
    private Quaternion _startRot = Quaternion.identity;
    // Update is called once per frame
    void Update()
    {
        //Set our Y to camera Y
        Vector3 pos = camera.position;
        if (PlayerStateController.instance.state == PlayerState.IDLE)
            pos.y += yOffset;
        else
            pos.y += yTowerOffset;
        transform.position = pos;


        if (_currentDampTime >= rotateDamping)
        {
            Vector3 dir = camera.forward;
            dir.y = 0f;
            dir = dir.normalized;
            float degrees = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            degrees = (degrees + rotateOffset) * -1f;
            Vector3 euler = new Vector3(0f, degrees, 0f);
            _startRot = transform.rotation;
            _target = Quaternion.Euler(euler);
            
            _currentDampTime = 0f;
        }
        
        /*if (Quaternion.Angle(_startRot, _target) < 45)
            return;*/

        transform.rotation = Quaternion.Lerp(_startRot, _target, (_currentDampTime / rotateDamping));

        _currentDampTime += Time.deltaTime;
    }
}
