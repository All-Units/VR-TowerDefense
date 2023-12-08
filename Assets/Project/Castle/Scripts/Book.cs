using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Book : MonoBehaviour
{
    public string title;
    public string author;
    public string description;
    public List<TextMeshProUGUI> titleTexts = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> authorTexts = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> descriptionTexts = new List<TextMeshProUGUI>();
    // Start is called before the first frame update
    void Start()
    {
        if (title == null) title = "Moby Dick";
        if (author == null) author = "Herman Melville";
        if (description == null) description = "A tale about whale";
        populateCover();
    }

    public void populateCover()
    {
        foreach (var text in titleTexts)
            text.text = title;
        foreach (var text in authorTexts)
            text.text = author;
        foreach (var text in descriptionTexts)
            text.text = description;
    }
}
