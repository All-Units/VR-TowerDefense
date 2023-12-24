using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StickPicture : MonoBehaviour
{
    [Header("Portrait")]
    [SerializeField] Sprite portrait;
    [Header("Component References")]
    [SerializeField] SpriteRenderer frontPicture;
    [SerializeField] SpriteRenderer backPicture;

    [SerializeField] Image speechBubble;
    [SerializeField] GameObject textParent;
    [SerializeField] TextMeshProUGUI speechText;

    [SerializeField]
    public string dialogueLine;
    // Start is called before the first frame update
    void Start()
    {
        frontPicture.sprite = portrait;
        backPicture.sprite = portrait;

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetText(string text)
    {
        textParent.SetActive(true);
        speechText.text = text;
    }
}
