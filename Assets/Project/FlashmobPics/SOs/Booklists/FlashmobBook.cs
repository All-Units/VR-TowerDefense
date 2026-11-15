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
    [SerializeField] SeriesStruct seriesData;
    public void SetBook(BookStruct bookStruct)
    {
        //Debug.Log($"setting: {bookStruct.title}", gameObject);
        spineTitle.text = $"{bookStruct.title} - {bookStruct.author}";
        description.text = bookStruct.bookDescription;
        cover.sprite = bookStruct.cover;
        book = bookStruct;
        if (BookshelfGenerator.seriesData.ContainsKey(book.series))
        {
            seriesData = BookshelfGenerator.seriesData[book.series];
            transform.localScale *= seriesData.coverSizeScalar;
        }
    }

    void Start()
    {
        bookRenderer.materials[2].color = book.coverColor;
        if (seriesData.seriesName != default(SeriesStruct).seriesName)
        {
            Debug.Log($"Setting cover to <color=#{ColorUtility.ToHtmlStringRGBA(seriesData.seriesColor)}>COLOR</color>");
            bookRenderer.materials[0].color = seriesData.seriesColor;
        }
    }
}
