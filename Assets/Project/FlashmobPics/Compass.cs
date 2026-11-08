using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Compass : MonoBehaviour
{
    [SerializeField] Transform needle;
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
        // We point at "North" - in this case, the world position of the anvil
        needle.LookAt(north);
        // We only want it rotating along its Y axis, so we remove the other rotation
        needle.localEulerAngles = new Vector3(0f, needle.localEulerAngles.y, 0f);
        // And that's it :)
    }
}
