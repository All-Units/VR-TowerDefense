using System.Collections;
using UnityEngine;

public class InventoryFollowCamera : MonoBehaviour
{
    
    
    [SerializeField] float timeToRotate = 0.5f;
    [SerializeField] float rotateThreshold = 30f;
    [SerializeField] float rotateOffset = -45f;
    Transform cam => InventoryManager.instance.playerCameraTransform;
    private void Start()
    {
        PlayerStateController.instance.OnPlayerTakeoverTower += _OnPlayerTakeoverTower;
    }
    void _OnPlayerTakeoverTower(PlayerControllableTower tower)
    {
        transform.position = tower.transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        float y = cam.eulerAngles.y;
        y = y.NormalizeAngle();
        float current = transform.eulerAngles.y;
        current = current.NormalizeAngle();

        float delta = current.ShortestDistanceToAngle(y);
        
        //If camera has greater angle than us, always offset
        bool offset = true;
        
        //If camera if to left of us (lower angle val), only snap if it's past the offset
        if (y < current)
        {
            offset = delta >= (rotateThreshold + rotateOffset);

        }
        if (delta >= rotateThreshold && offset)
        {
            _SnapToTarget(y);
        }
        return;
        /*
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
        
        if (Quaternion.Angle(_startRot, _target) < 45)
            return;

        transform.rotation = Quaternion.Lerp(_startRot, _target, (_currentDampTime / rotateDamping));

        _currentDampTime += Time.deltaTime;*/
    }
    void _SnapToTarget(float targetY)
    {
        if (_currentRotator != null)
            StopCoroutine(_currentRotator);
        _currentRotator = _RotateToCamera(targetY);
        StartCoroutine(_currentRotator);
    }
    IEnumerator _currentRotator = null;
    IEnumerator _RotateToCamera(float targetY)
    {
        float startY = transform.eulerAngles.y;
        
        float t = 0f;
        while (t < timeToRotate)
        {
            t += Time.deltaTime;
            float y = Mathf.Lerp(startY, targetY, (t / timeToRotate));
            transform.eulerAngles = new Vector3(0f, y, 0f);
            yield return null;
        }

    }
}
