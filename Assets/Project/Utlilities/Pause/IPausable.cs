using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

/// <summary>
/// All IPausable objects freeze OnPause, and resume OnUnPause
/// 
/// </summary>
public interface IPausable
{
    /*
    private IPausableComponents _ipComponents = null;
    public IPausableComponents IPComponents { 
        get {
            if (_ipComponents == null) _ipComponents = this.GetPausableComponents();
            return _ipComponents; 
        } 
    }

     */
    public IPausableComponents IPComponents { get; }
    GameObject gameObject { get; }
    public abstract void OnInitPausable();
    public abstract void OnDestroyPausable();
   
    public abstract void OnPause();
    
    
    abstract void OnResume();
}




