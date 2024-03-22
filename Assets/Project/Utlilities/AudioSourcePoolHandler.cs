using UnityEngine;

public class AudioSourcePoolHandler : MonoBehaviour
{
    private AudioSourcePool _shotSourcePool;
    [SerializeField] private AudioSource shotClipControllerPrefab;

    private void Awake()
    {
        _shotSourcePool = new AudioSourcePool(shotClipControllerPrefab);
    }
    public void Play()
    {
        _shotSourcePool.Play(transform.position);
    }

    public void PlayAt(Vector3 pos)
    {
        _shotSourcePool.Play(pos);
    }
}