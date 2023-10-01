using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public int damage;
    public float speed = 10f;

    private Rigidbody _rigidbody;
    private bool _inAir = false;
    private bool _isDestroying = false;
    [SerializeField] private ParticleSystem particles;
    public StatusModifier statusModifier;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        PullInteraction.PullActionReleased += PullInteractionOnPullActionReleased;

        Stop();
        
        Destroy(gameObject, 5f);
    }

    private void OnDestroy()
    {
        PullInteraction.PullActionReleased -= PullInteractionOnPullActionReleased;
    }

    private void PullInteractionOnPullActionReleased(float obj)
    {
        PullInteraction.PullActionReleased -= PullInteractionOnPullActionReleased;
        Fire(obj);
    }

    public void Fire()
    {
        Fire(1);
    }

    private void Fire(float obj)
    {
        gameObject.transform.parent = null;
        _inAir = true;
        particles.gameObject.SetActive(true);
        SetPhysics(true);

        var force = transform.forward * obj * speed;
        _rigidbody.AddForce(force, ForceMode.Impulse);
        StartCoroutine(RotateWithVelocity());
    }

    private IEnumerator RotateWithVelocity()
    {
        yield return new WaitForFixedUpdate();
        while (_inAir)
        {
            var newRot = Quaternion.LookRotation(_rigidbody.velocity, transform.up);
            transform.rotation = newRot;
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        //Debug.Log($"Hit {other.gameObject}", other.gameObject);
        if (_isDestroying) return;
        
        var colliderGameObject = other.collider.gameObject;
        
        if (colliderGameObject.TryGetComponent(out HealthController healthController))
        {
            healthController.TakeDamage(damage);
            
            if(statusModifier)
            {
                var statusEffectController = healthController.GetComponentInChildren<StatusEffectController>();
                if(statusEffectController)
                    statusModifier.ApplyStatus(statusEffectController);
            }
        }

        _isDestroying = true;
        particles.transform.SetParent(null);
        particles.Stop();

        Destroy(gameObject);
    }

    private void Stop()
    {
        _inAir = false;
        particles.gameObject.SetActive(false);
        SetPhysics(false);
    }

    private void SetPhysics(bool b)
    {
        _rigidbody.useGravity = b;
        _rigidbody.isKinematic = !b;
    }
}