using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IPausableComponents 
{ 
    
    public IPausableComponents(List<Rigidbody> rigidbodies, List<ParticleSystem> particleSystems)
    {
        this.rigidbodies = rigidbodies;
        this.particleSystems = particleSystems;
    }
    public List<Rigidbody> rigidbodies;
    public Dictionary<Rigidbody, _rb_Frame> rigidbodyCache = new Dictionary<Rigidbody, _rb_Frame>();
    public List<ParticleSystem> particleSystems;
    public struct _rb_Frame
    {
        public _rb_Frame(Rigidbody rb)
        {
            this.velocity = rb.velocity;
            this.constraints = rb.constraints;
        }
        public Vector3 velocity;
        public RigidbodyConstraints constraints;
    }
}

public static class IPExtenstions
{
    /// <summary>
    /// Returns a new IPausableComponents obj
    /// </summary>
    /// <param name="pausable"></param>
    /// <returns></returns>
    public static IPausableComponents GetPausableComponents(this IPausable pausable)
    {
        var rbs = pausable.gameObject.GetComponentsInChildren<Rigidbody>(true).ToList();
        var particles = pausable.gameObject.GetComponentsInChildren<ParticleSystem>(true).ToList();


        return new IPausableComponents(rbs, particles);
    }
    public static void InitPausable(this IPausable pausable)
    {
        XRPauseMenu.OnPause += pausable.OnPause;

        XRPauseMenu.OnResume += pausable.OnResume;
    }
    public static void DestroyPausable(this IPausable pausable)
    {
        XRPauseMenu.OnPause -= pausable.OnPause;

        XRPauseMenu.OnResume -= pausable.OnResume;
    }
    #region PauseFunctions
    public static void BaseOnPause(this IPausable pausable)
    {
        //Freeze all rigidbodies
        _PauseRBs(pausable.IPComponents);

        //Freeze all particles
        _PauseParticles(pausable.IPComponents);
        Debug.Log($"Base on pause for GO {pausable.gameObject.name}, it had {pausable.IPComponents.rigidbodies.Count} rigidbodies");
    }
    static void _PauseRBs(IPausableComponents components)
    {
        var cache = components.rigidbodyCache;
        cache.Clear();
        foreach(var rb in components.rigidbodies)
        {
            var frame = new IPausableComponents._rb_Frame(rb);
            cache.Add(rb, frame);
            Debug.Log($"Cached RB velocity as {rb.velocity}, or {frame.velocity}");
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        components.rigidbodyCache = cache;
    }

    static void _PauseParticles(IPausableComponents components)
    {
        foreach (var particle in components.particleSystems)
        {
            particle.Pause();
        }
    }
    #endregion

    #region ResumeFunctions
    public static void BaseOnResume(this IPausable pausable)
    {
        //Resume RBs
        _ResumeRBs(pausable.IPComponents);

        _ResumeParticles(pausable.IPComponents);

    }
    static void _ResumeRBs(IPausableComponents components)
    {
        foreach (var kv in components.rigidbodyCache)
        {
            Rigidbody rb = kv.Key;
            var frame = kv.Value;
            rb.velocity = frame.velocity;
            rb.constraints = frame.constraints;
            rb.velocity = frame.velocity;
            Debug.Log($"Restored RB velocity to {rb.velocity}, or {frame.velocity}");
        }
        components.rigidbodyCache.Clear();
    }
    static void _ResumeParticles(IPausableComponents components)
    {
        foreach (var particle in components.particleSystems)
        {
            particle.Play();
        }
    }
    #endregion
}
