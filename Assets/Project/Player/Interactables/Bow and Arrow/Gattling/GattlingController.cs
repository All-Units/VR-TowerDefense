using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GattlingController : MonoBehaviour
{
    public float spinUpTime = 3.5f;
    public float maxSpinSpeed = 15f;
    public Transform barrels;
    public Projectile projectile;
    public Transform launchPoint;
    
    private Coroutine _spinRoutine;
    private bool _isActive;
    private float fireRate;
    
    public void OnActivate()
    {
        _isActive = true;
        if (_spinRoutine == null)
            _spinRoutine = StartCoroutine(Spin());
    }

    public void OnDeactivate()
    {
        _isActive = false;
    }

    private IEnumerator Spin()
    {
        var t = 0f;
        do
        {
            t = _isActive ? t + Time.deltaTime : t - Time.deltaTime;
            t = Mathf.Clamp(t, 0, spinUpTime);
            
            var spinSpeed = Mathf.Lerp(0, maxSpinSpeed, t / spinUpTime);
            barrels.Rotate(Vector3.up, spinSpeed);
            if (spinSpeed > maxSpinSpeed / 4 && _isActive)
            {
                fireRate += spinSpeed;

                if (fireRate >= 180)
                {
                    var p = Instantiate(projectile, launchPoint.position, launchPoint.rotation);
                    p.Fire();
                    fireRate -= 180;
                }
            }
            
            yield return null;
        } while (t > 0);

        _spinRoutine = null;
    }
}
