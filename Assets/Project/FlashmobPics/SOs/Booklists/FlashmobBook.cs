using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class FlashmobBook : MonoBehaviour
{
    [SerializeField] TextMeshPro spineTitle;
    [SerializeField] TextMeshPro spineAuthor;
    [SerializeField] TextMeshPro description;
    [SerializeField] TextMeshPro titleLabel;
    [SerializeField] TextMeshPro authorLabel;
    [SerializeField] Image cover;
    [SerializeField] MeshRenderer bookRenderer;
    [HideInInspector]
    [SerializeField] BookStruct book;
    [SerializeField] SeriesStruct seriesData;
    public void SetBook(BookStruct bookStruct)
    {
        //Debug.Log($"setting: {bookStruct.title}", gameObject);
        spineTitle.text = $"{bookStruct.title}";
        spineAuthor.text = bookStruct.author;
        description.text = bookStruct.bookDescription;
        cover.sprite = bookStruct.cover;
        book = bookStruct;
        titleLabel.text = book.DisplayTitle ?  book.title : "";
        authorLabel.text = book.DisplayAuthor ? book.author : "";
        if (BookshelfGenerator.seriesData.ContainsKey(book.series))
        {
            seriesData = BookshelfGenerator.seriesData[book.series];
            transform.localScale *= seriesData.coverSizeScalar;
        }
        name = book.title;
    }

    XRGrabInteractable grab;
    Rigidbody rb;
    Canvas canvas;
    MeshCollider meshCollider;

    [SerializeField] float lodUpdateTime = 1f;
    [SerializeField] float minDistanceToPlayer = 12f;
    void Start()
    {
        bookRenderer.materials[2].color = book.coverColor;
        if (seriesData.seriesName != default(SeriesStruct).seriesName)
        {
            bookRenderer.materials[0].color = seriesData.seriesColor;
        }
        grab = GetComponentInChildren<XRGrabInteractable>();
        rb = GetComponentInChildren<Rigidbody>();
        canvas = GetComponentInChildren<Canvas>();
        meshCollider = GetComponentInChildren<MeshCollider>();
        startConstraints = rb.constraints;

        StartCoroutine(_LODLoop());
    }
    RigidbodyConstraints startConstraints;

    [SerializeField] float _lastDistance = -1f;

    IEnumerator _LODLoop()
    {
        yield return new WaitForSeconds(Random.Range(lodUpdateTime * 2f, lodUpdateTime * 3f));
        while (true)
        {
            float distance = Vector3.Distance(transform.position, KeepPlayerInTower.playerPos);
            _lastDistance = distance;
            bool active = distance <= minDistanceToPlayer;
            grab.enabled = active;
            // isKinematic is inverse!!!
            rb.isKinematic = !active;
            rb.useGravity = active;
            rb.constraints = active ? startConstraints : RigidbodyConstraints.FreezeAll;
            meshCollider.enabled = active;
            canvas.gameObject.SetActive(active);
            yield return new WaitForSeconds(lodUpdateTime);
        }
    }
}
