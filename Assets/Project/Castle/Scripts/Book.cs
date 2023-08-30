using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Book : MonoBehaviour
{
    public string title;
    public string author;
    public List<TextMeshProUGUI> titleTexts = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> authorTexts = new List<TextMeshProUGUI>();
    // Start is called before the first frame update
    void Start()
    {
        populateCover();
    }

    void populateCover()
    {
        foreach (var text in titleTexts)
            text.text = title;
        foreach (var text in authorTexts)
            text.text = author;
    }
}
