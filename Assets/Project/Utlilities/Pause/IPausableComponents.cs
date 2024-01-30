using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IPausableComponents 
{ 
    
    public IPausableComponents(List<Rigidbody> rigidbodies, 
        List<ParticleSystem> particleSystems, List<Animator> animators)
    {
        this.rigidbodies = rigidbodies;
        this.particleSystems = particleSystems;
        this.animators = animators;
    }
    public List<Rigidbody> rigidbodies;
    public Dictionary<Rigidbody, _rb_Frame> rigidbodyCache = new Dictionary<Rigidbody, _rb_Frame>();
    public List<ParticleSystem> particleSystems;
    public List<Animator> animators = new List<Animator>();
    public Dictionary<Animator, float> animCache = new Dictionary<Animator, float> { };

    public Dictionary<TrailRenderer, float> trailCache = new Dictionary<TrailRenderer, float> { };
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
        var animators = pausable.gameObject.GetComponentsInChildren<Animator>(true).ToList();


        return new IPausableComponents(rbs, particles, animators);
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


        //Pause anims
        _PauseAnims(pausable.IPComponents);
        var trails = pausable.gameObject.GetComponentsInChildren<TrailRenderer>(true).ToList();
        var cache = pausable.IPComponents.trailCache;
        cache.Clear();
        foreach (var trail in trails)
        {
            cache.Add(trail, trail.time);
            trail.time = float.MaxValue;
        }
            


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
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        components.rigidbodyCache = cache;
    }

    static void _PauseParticles(IPausableComponents components)
    {
        foreach (var particle in components.particleSystems)
        {
            if (particle.isPlaying)
                particle.Pause();
        }
    }

    static void _PauseAnims(IPausableComponents components)
    { 
        var cache = new Dictionary<Animator, float>();
        foreach(var anim in components.animators)
        {
            cache.Add(anim, anim.speed);
            anim.speed = 0f;
        }
        components.animCache = cache;
    }


    #endregion

    #region ResumeFunctions
    public static void BaseOnResume(this IPausable pausable)
    {
        //Resume RBs
        _ResumeRBs(pausable.IPComponents);

        _ResumeParticles(pausable.IPComponents);

        _ResumeAnims(pausable.IPComponents);
        foreach (var trail in pausable.IPComponents.trailCache)
        {
            trail.Key.time = trail.Value;
        }
        pausable.IPComponents.trailCache.Clear();

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
        }
        components.rigidbodyCache.Clear();
    }
    static void _ResumeParticles(IPausableComponents components)
    {
        foreach (var particle in components.particleSystems)
        {
            if (particle.isPaused)
                particle.Play();
        }
    }
    static void _ResumeAnims(IPausableComponents components)
    {
        foreach ( var kv in components.animCache)
        {
            var anim = kv.Key;
            anim.speed = kv.Value;
        }
        components.animCache.Clear();
    }
    #endregion
}
