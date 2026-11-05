using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Compass : MonoBehaviour
{
    [SerializeField] Transform needle;
    [SerializeField] float angleOffset = 90f;
    /// <summary>
    /// The Anvil class either returns its pos, or "North"
    /// </summary>
    Vector3 north => Anvil.position;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _pointNeedle();
    }
    void _pointNeedle()
    {
        Vector3 dir = transform.position - north;
        dir.y = 0f;
        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        angle += angleOffset;
        
        //dir = north;
        //dir.y = needle.position.y;
        print($"Angle was: {needle.localEulerAngles.y}, changing to {angle} bc of dir {dir} ({transform.position} - {north})");
        needle.localEulerAngles = new Vector3(0f, angle, 0f);
        //needle.LookAt(dir, Vector3.up);

    }
}
