using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Anvil : MonoBehaviour
{
    public static Anvil anvil;
    /// <summary>
    /// If there is no instance, returns "North" -  (float.PositiveInfinity, 0f, 0f)
    /// otherwise returns position of the anvil
    /// </summary>
    public static Vector3 position
    {
        get
        {
            if (anvil == null)
            {
                return new Vector3(10000000, 0f, 0f);
            }
            return anvil.transform.position;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        _setAnvil();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isPlaying == false && anvil == null)
        {
            _setAnvil();
        }
    }
    void _setAnvil()
    {
        if (anvil != null && anvil != this)
        {
            Destroy(anvil);
            anvil = null;
        }
        anvil = this;
    }
}
