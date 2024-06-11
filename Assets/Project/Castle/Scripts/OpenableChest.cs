using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OpenableChest : MonoBehaviour
{
    public Animator anim;
    public AudioClipController sfx;
    public TextMeshProUGUI labelText;

    [SerializeField] GameObject contents;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < contents.transform.childCount; i++)
        {
            Transform t = contents.transform.GetChild(i);
            _startPositions.Add(t, new _pos(t));
        }
        contentsPos = contents.transform.position;
        contents.transform.position = contentsPos + (Vector3.down * 1000f);
        

        if (anim == null) anim = GetComponent<Animator>();
        if (sfx == null)  sfx = GetComponent<AudioClipController>();
       
        
    }

    Vector3 contentsPos;
    Dictionary<Transform, _pos> _startPositions = new Dictionary<Transform, _pos>();
    struct _pos
    {
        public Vector3 position;
        public Quaternion rotation;
        public _pos(Transform t)
        {
            this.position = t.position;
            this.rotation = t.rotation;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    bool _firstOpened = false;
    public void ButtonClicked()
    {
        anim.SetTrigger("OpenChest");
        sfx.PlayClip();
        if (_firstOpened == false)
        {
            _firstOpened = true;
            contents.SetActive(true);
            StartCoroutine(_FirstOpenRoutine());
        }
    }
    IEnumerator _FirstOpenRoutine()
    {
        contents.transform.position = contentsPos;
        foreach (var t in _startPositions)
        {
            t.Key.position = t.Value.position;
            t.Key.rotation = t.Value.rotation;
        }
        yield return null;
        foreach (var t in _startPositions)
        {
            t.Key.position = t.Value.position;
            t.Key.rotation = t.Value.rotation;
        }
    }
    public void OnHoverStart()
    {
        labelText.gameObject.SetActive(true);
    }
    public void OnHoverEnd()
    {
        labelText.gameObject.SetActive(false);
    }
    public void OnChestOpen()
    {
        labelText.text = "CLOSE CHEST";
    }
    public void OnChestClose()
    {
        labelText.text = "OPEN CHEST";
    }


}
