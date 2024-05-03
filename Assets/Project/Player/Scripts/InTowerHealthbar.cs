using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InTowerHealthbar : MonoBehaviour
{
    [SerializeField] GameObject healthbarParent;
    [SerializeField] float rotateSpeed = 5f;
    [SerializeField] float timeToRotate = 2f;
    [SerializeField] float rotateThreshold = 30f;

    Slider slider;
    Transform cam => InventoryManager.instance.playerCameraTransform;
    // Start is called before the first frame update
    void Start()
    {
        slider = healthbarParent.GetComponentInChildren<Slider>();
        PlayerStateController.instance.OnPlayerTakeoverTower += _OnTowerTakeover;
        PlayerStateController.OnStateChange += _OnStateChange;
        healthbarParent.SetActive(false);
    }
    private void OnDisable()
    {
        PlayerStateController.OnStateChange -= _OnStateChange;
    }

    // Update is called once per frame
    void Update()
    {
        if (healthbarParent.activeInHierarchy)
        {
            float y = cam.eulerAngles.y;
            float current = transform.eulerAngles.y;
            float delta = current.ShortestDistanceToAngle(y);
            
            if (delta >= rotateThreshold)
            {
                _SnapToTarget(y);
            }
        }
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
        //If we're currently less than target, we need to add
        //If we're more, subtract
        int sign = (startY < targetY) ? 1 : -1;

        float t = 0f;
        while (t < timeToRotate)
        {
            t += Time.deltaTime;
            float y = Mathf.Lerp(startY, targetY, (t / timeToRotate));
            transform.eulerAngles = new Vector3(0f, y, 0f);
            yield return null;
        }
        
    }
    public void EjectFromTower()
    {
        Vector3 pos = cam.position + cam.forward * 3f;
        pos.y -= 2f;
        Vector3 rot = new Vector3(0f, cam.eulerAngles.y, 0f);
        PlayerStateController.TeleportPlayerToPoint(pos, Quaternion.Euler(rot));
        PlayerStateController.ReleaseControlOfTower();
    }
    void _PositionHealthbar(Transform target)
    {
        healthbarParent.SetActive(true);

        transform.position = target.position;
    }
    void _UpdateValue(int current)
    {
        slider.value = (float)current / (float)_currentTower.healthController.MaxHealth;
    }
    PlayerControllableTower _currentTower = null;
    void _OnTowerTakeover(PlayerControllableTower tower)
    {
        if (_currentTower != null)
        {
            _currentTower.healthController.OnTakeDamage -= _UpdateValue;
        }
        _currentTower = tower;
        tower.healthController.OnTakeDamage += _UpdateValue;
        _UpdateValue(tower.healthController.CurrentHealth);
        _PositionHealthbar(tower.transform);
    }
    void _OnStateChange(PlayerState oldState, PlayerState newState)
    {
        
        //Do nothing if we are not changing to idle
        if (newState != PlayerState.IDLE) return;
        healthbarParent.SetActive(false);
    }
}
