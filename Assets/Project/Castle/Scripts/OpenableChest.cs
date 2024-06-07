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
    // Start is called before the first frame update
    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (sfx == null)  sfx = GetComponent<AudioClipController>();
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ButtonClicked()
    {
        anim.SetTrigger("OpenChest");
        sfx.PlayClip();
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
