using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public int damage;
    public float speed = 10f;

    private Rigidbody _rigidbody;
    private bool _inAir = false;
    private bool _isDestroying = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        PullInteraction.PullActionReleased += PullInteractionOnPullActionReleased;

        Stop();
    }

    private void OnDestroy()
    {
        PullInteraction.PullActionReleased -= PullInteractionOnPullActionReleased;
    }

    private void PullInteractionOnPullActionReleased(float obj)
    {
        PullInteraction.PullActionReleased -= PullInteractionOnPullActionReleased;
        gameObject.transform.parent = null;
        _inAir = true;
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
        Debug.Log($"Hit {other.gameObject}", other.gameObject);
        if (_isDestroying) return;
        
        var colliderGameObject = other.collider.gameObject;

        if (colliderGameObject.TryGetComponent(out HealthController healthController))
        {
            healthController.TakeDamage(damage);
        }

        _isDestroying = true;
        Destroy(gameObject);
    }

    private void Stop()
    {
        _inAir = false;
        SetPhysics(false);
    }

    private void SetPhysics(bool b)
    {
        _rigidbody.useGravity = b;
        _rigidbody.isKinematic = !b;
    }
}