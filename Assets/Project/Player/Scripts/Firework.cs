using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Firework : MonoBehaviour
{
    [Header("Gameplay variables")]
    public float DetonateHeight = 25f;
    public float DetonateVar = 15f;
    public float moveSpeed = 3f;

    [SerializeField] List<Color> colorList = new List<Color>() 
    { Color.cyan, Color.magenta, Color.yellow, Color.red, Color.green};

    [Header("Obj references")]
    [SerializeField] ParticleSystem fireworkSystem;
    [SerializeField] ParticleSystem explosion;
    [SerializeField] AudioClipController sfx;



    float startY = 0f;
    Vector3 horizontalDelta = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        DetonateHeight += Random.Range(DetonateVar * -1f, DetonateVar);
        startY = transform.position.y;
        var main = fireworkSystem.main;
        horizontalDelta.x += Random.Range(2 * -1f, 2);
        horizontalDelta.z += Random.Range(2 * -1f, 2);
        main.startColor = colorList.GetRandom();
        StartCoroutine(_LaunchFirework());
    }
    IEnumerator _LaunchFirework()
    {
        while (true)
        {
            Vector3 dir = new Vector3(0f, moveSpeed * Time.deltaTime, 0f);
            dir.x += horizontalDelta.x * Time.deltaTime;
            dir.z += horizontalDelta.z * Time.deltaTime;
            if (XRPauseMenu.IsPaused == false)
                transform.Translate(dir);
            if (transform.position.y >= startY + DetonateHeight)
                break;
            yield return null;
        }
        fireworkSystem.Play();
        explosion.Play();
        sfx.PlayClip();
        
    }

    static float _distanceFromPlayer = 0f;
    static float _distanceVar = 80f;
    public static void SpawnFirework()
    {
        if (InventoryManager.instance == null) return;
        Transform cam = InventoryManager.instance.playerCameraTransform;
        Vector3 center = cam.position;
        Vector3 dir = cam.forward; dir.y = 0f;
        center += (dir *= _distanceFromPlayer);
        center.x += Random.Range(_distanceVar * -1f, _distanceVar); 
        center.z += Random.Range(_distanceVar * -1f, _distanceVar);
        center.y += 100f;
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Ground");
        if (Physics.Raycast(center, Vector3.down, out hit, float.PositiveInfinity, mask))
        {
            GameObject firework = Instantiate(Resources.Load<GameObject>("Prefabs/Firework"));
            center.y = hit.point.y;
            firework.transform.position = center;
            firework.DestroyAfter(10f);
        }
    }
    
}
