using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabTutorial : MonoBehaviour
{
    public GameObject grabObjectsParent;
    public float verticalOffset = 1f;
    public float horizontalOffset = 2f;


    [Header("Object references")]
    public GameObject box;
    public XRGrabInteractable ammo;

    [Header("Text References")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI hint;
    [TextArea]
    public string firstGrab = "Now drop the cannonball in the crate";

    [TextArea] public string firstHint = "Hint:\nRelease your grip";
    [TextArea] public string secondMessage = "Congratulations!";
    public int targetNumber = 3;

    public Color incompleteColor;
    public Color completeColor;

    Vector3 ammoStartLocalPos;
    private void Awake()
    {
        box.SetActive(false);
        
        ammoStartLocalPos = ammo.transform.localPosition;
        
    }
    private void OnEnable()
    {
        _RecenterGrabbables();
        grabObjectsParent.SetActive(true);
        IEnumerator _Wait()
        {
            yield return null;
            ammo.firstSelectEntered.AddListener(_OnFirstGrab);
            ammo.gameObject.SetActive(false);
            _SpawnNewAmmo();
        }
        StartCoroutine(_Wait());
        
        if (TutorialManager.Instance != null )
            TutorialManager.Instance.OnRecenter += _RecenterGrabbables;
        

        
    }
    private void OnDisable()
    {
        grabObjectsParent.SetActive(false);
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnRecenter -= _RecenterGrabbables;

        foreach (var spawned in spawnedAmmo)
        {
            if (spawned)
                Destroy(spawned);
        }
    }
    Transform cam => InventoryManager.instance?.playerCameraTransform;
    Transform parent => grabObjectsParent.transform;
    void _RecenterGrabbables()
    {
        if (cam == null) return; 
        Vector3 pos = cam.position;
        Vector3 offset = cam.forward;
        offset.y = 0f; offset = offset.normalized;
        offset *= horizontalOffset;
        pos += offset;
        pos.y -= verticalOffset;
        Vector3 euler = parent.eulerAngles;
        euler.y = cam.eulerAngles.y;

        parent.eulerAngles = euler;

        parent.position = pos;
    }

    HashSet<IXRSelectInteractable> grabbedAmmo = new HashSet<IXRSelectInteractable> ();
    bool _anyGrabbed = false;
    void _OnFirstGrab(SelectEnterEventArgs a)
    {
        box.SetActive(true);
        IXRSelectInteractable table = a.interactableObject;
        table.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        //Do nothing if we've already grabbed this ammo
        if (grabbedAmmo.Contains(table)) return;
        grabbedAmmo.Add(table);
        IEnumerator _Wait()
        {
            yield return new WaitForSeconds(1f);
            
            _SpawnNewAmmo();
        }
        StartCoroutine(_Wait());

        if (_anyGrabbed == false)
        {
            _anyGrabbed = true;
            title.text = firstGrab;
            hint.text = firstHint;
        }

    }

    int cannonballsPlaced = 0;
    public void OnCannonballEnterBox()
    {
        cannonballsPlaced++;
        string message = secondMessage;
        Color c = (cannonballsPlaced >= targetNumber) ? completeColor : incompleteColor;
        string color = ColorUtility.ToHtmlStringRGBA(c);
        message += $"\n<color=#{color}>{cannonballsPlaced} / {targetNumber}</color>\n";
        title.text = message;
        hint.text = "";

        if (cannonballsPlaced >= targetNumber)
        {
            IEnumerator _Wait()
            {
                yield return new WaitForSeconds(0.8f);

                TutorialManager.SetSkip();
            }
            StartCoroutine(_Wait());
        }
            
    }
    List<GameObject> spawnedAmmo = new List<GameObject>();
    void _SpawnNewAmmo()
    {
        //Spawn a new grenade
        var spawned = Instantiate(ammo, parent);
        spawned.firstSelectEntered.RemoveAllListeners();
        spawned.firstSelectEntered.AddListener(_OnFirstGrab);
        spawned.gameObject.SetActive(true);
        spawned.transform.localPosition = ammoStartLocalPos;
        spawned.transform.localRotation = Quaternion.identity;
        spawnedAmmo.Add(spawned.gameObject);

    }
}
