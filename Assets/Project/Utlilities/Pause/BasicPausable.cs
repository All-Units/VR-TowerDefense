using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPausable : MonoBehaviour, IPausable
{
    private IPausableComponents _ipComponents = null;
    public IPausableComponents IPComponents
    {
        get
        {
            if (_ipComponents == null) _ipComponents = this.GetPausableComponents();
            return _ipComponents;
        }
    }
    void OnDestroy()
    {
        OnDestroyPausable();
    }
    public void OnDestroyPausable()
    {
        this.DestroyPausable();        
    }

    public void OnInitPausable()
    {
        this.InitPausable();
    }

    public void OnPause()
    {
        this.BaseOnPause();
    }

    public void OnResume()
    {
        this.BaseOnResume();
    }

    // Start is called before the first frame update
    void Start()
    {
        OnInitPausable();
    }

}
