using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

public class TrailerCamMover : MonoBehaviour
{
    public Transform TitleLayoutParent;
    public float TimeToGrow = 0.4f;
    public float TimeBetween = 0.1f;
    public AnimationCurve growCurve;
    
    public GameObject playerPref;
    public float moveSpeed;
    public Vector3 moveDir = new Vector3(1f, 0f, 0f);

    public float rotateSpeed = 2f;
    public Vector3 rotateDir = new Vector3(1f, 0f, 0f);
    public bool isMoving = false;
    // Start is called before the first frame update
    void Start()
    {
        if (playerPref == null)
            playerPref = FindObjectOfType<InventoryManager>().gameObject;
        if (playerPref.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }
        StartCoroutine(_TowerTimeLapse());
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Time.timeScale = 0.3f;
            print($"Time scale {Time.timeScale}");
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            Time.timeScale = 0.5f;
            print($"Time scale {Time.timeScale}");
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            Time.timeScale = 0.7f;
            print($"Time scale {Time.timeScale}");
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            Time.timeScale = 1f;
            print("Time scale NORMAL");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isMoving = !isMoving;
            _PauseAll();
        } 
        if (isMoving)
        {
            Vector3 pos = transform.localPosition;
            var dir = transform.forward; dir.y = 0f;
            pos += (dir * moveSpeed * Time.deltaTime);
            //transform.localPosition = pos;
            transform.localPosition += (moveDir * moveSpeed * Time.deltaTime);

            Vector3 rot = transform.localEulerAngles;
            rot += (rotateDir * rotateSpeed * Time.deltaTime);
            transform.localEulerAngles = rot;
        }
    }
    void _PauseAll()
    {
        var particles = FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
        foreach (var particle in particles)
        {
            particle.Pause();
        }
        var anims = FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (var anim in anims)
        { 
            anim.speed = 0f;
            var e = anim.GetComponentInChildren<Enemy>();
            if (e == null) continue;
            foreach (var rb in e.GetComponentsInChildren<Rigidbody>())
                rb.constraints = RigidbodyConstraints.FreezeAll;
            e.CanMove = false;
            e.RB.constraints = RigidbodyConstraints.FreezeAll;
        }
        foreach (var tower in FindObjectsByType<ProjectileTower>(FindObjectsSortMode.None))
            tower.CanFire = false;


    }
    IEnumerator _TowerTimeLapse()
    {
        if (TitleLayoutParent == null)
            yield break;
        
        List<Transform> texts = new List<Transform>();
        for (int i = 0; i < TitleLayoutParent.childCount; i++)
        {
            Transform t = TitleLayoutParent.GetChild(i);
            t.gameObject.SetActive(false);
            t.localScale = Vector3.zero;
            texts.Add(t);
        }
        yield return new WaitForSeconds(1f);
        foreach (Transform text in texts)
        {
            float time = 0f;
            text.gameObject.SetActive(true);
            while (time <= TimeToGrow)
            {
                yield return null;
                time += Time.deltaTime;
                float t = time / TimeToGrow;
                float scale = Mathf.Lerp(0, 1, t);
                scale = growCurve.Evaluate(t);
                text.localScale = Vector3.one * scale;
                float x = Mathf.Lerp(90f, 0f, t);
                text.localEulerAngles = new Vector3(x, 0f, 0f);
            }
            yield return new WaitForSeconds(TimeBetween);
        }
    }
}
