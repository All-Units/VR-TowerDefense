using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlashmobBook : MonoBehaviour
{
    [SerializeField] TextMeshPro spineTitle;
    [SerializeField] TextMeshPro description;
    [SerializeField] Image cover;
    public void SetBook(BookStruct bookStruct)
    {
        spineTitle.text = $"{bookStruct.title} - {bookStruct.author}";
        description.text = bookStruct.bookDescription;
        cover.sprite = bookStruct.cover;
    }
}
