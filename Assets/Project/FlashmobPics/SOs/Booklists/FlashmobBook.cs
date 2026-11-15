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
    [SerializeField] MeshRenderer bookRenderer;
    [HideInInspector]
    [SerializeField] BookStruct book;
    public void SetBook(BookStruct bookStruct)
    {
        //Debug.Log($"setting: {bookStruct.title}", gameObject);
        spineTitle.text = $"{bookStruct.title} - {bookStruct.author}";
        description.text = bookStruct.bookDescription;
        cover.sprite = bookStruct.cover;
        book = bookStruct;
    }

    void Start()
    {
        bookRenderer.materials[2].color = book.coverColor;
    }
}
