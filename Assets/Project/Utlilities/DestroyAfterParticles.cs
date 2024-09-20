using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyAfterParticles : MonoBehaviour
{
    public bool SetParentNull = true;
    [HideInInspector]
    [SerializeField] List<ParticleSystem> particleSystems = new List<ParticleSystem>();
#if UNITY_EDITOR
    [ExecuteInEditMode]
    private void OnEnable()
    {
        _InitParticles();
    }
#endif
    void _InitParticles() { if (particleSystems.Count == 0) particleSystems = GetComponentsInChildren<ParticleSystem>().ToList(); }
    // Start is called before the first frame update
    void Start()
    {
        float longest = 0f;
        _InitParticles();
        foreach (ParticleSystem ps in particleSystems)
        {
            float duration = ps.main.duration;
            if (duration > longest) longest = duration;

        }
        if (SetParentNull)
            transform.parent = null;
        gameObject.DestroyAfter(longest);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
