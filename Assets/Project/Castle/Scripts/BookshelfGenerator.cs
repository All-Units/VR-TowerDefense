using System;
using System.Collections.Generic;
using UnityEditor;

using Random = UnityEngine.Random;
using UnityEngine;
#if UNITY_EDITOR




[CustomEditor(typeof(BookshelfGenerator))]
class _bookGenInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Regenerate shelving"))
        {
            ((BookshelfGenerator)target).RegenShelves();
        }
        if (GUILayout.Button("Regenerate ALL shelving"))
        {
            var g = (BookshelfGenerator)target;
            g.RegenAllShelving();
            
        }
        base.OnInspectorGUI();
    }
}
#endif
public class BookshelfGenerator : MonoBehaviour
{
    public Transform booksParent;
    public Transform rightBound;
    [SerializeField] GameObject bookPrefab;
    public int skipPercent = 40;
    [Tooltip("Lower and upper bounds for distance between books")]
    public Vector2 lerpRange = new Vector2(.1f, .2f);
    [SerializeField] private BooklistSO booklist;

#if UNITY_EDITOR
    public void RegenAllShelving()
    {
        var shelves = transform.root.GetComponentsInChildren<BookshelfGenerator>();
        foreach (var gen in shelves)
        {
            //BookshelfGenerator gen = c.GetComponent<BookshelfGenerator>();
            gen.RegenShelves();
        }
    }
    Transform tempParent;
    public void RegenShelves()
    {
        i = 0;
        print("Regen shelving CLICKED");
        _clearShelves();
        foreach (Transform shelf in booksParent)
            _generateShelf(shelf);
    }

    void _clearShelves()
    {
        foreach (Transform shelf in booksParent)
        {
            int i = shelf.childCount - 1;
            while (i >= 0)
            {
                Transform book = shelf.GetChild(i);
                DestroyImmediate(book.gameObject);
                i = shelf.childCount - 1;
            }
        }
    }

    private List<int> angleOffsets = new List<int>() { -10, 0, 0, 0, 10 };

    int i = 0;
    void _generateShelf(Transform shelf)
    {
        Vector3 start = shelf.position;
        Vector3 end = rightBound.position;
        end.y = start.y;
        float t = 0;
        while (t < 1)
        {
            t += Random.Range(lerpRange.x, lerpRange.y);
            if (t >= 1)
                break;
            //Skip some of our placements
            skipPercent = Mathf.Clamp(skipPercent, 0, 100);
            if (skipPercent >= Random.Range(0, 100))
                continue;
            Vector3 pos = Vector3.Lerp(start, end, t);
            GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(bookPrefab, shelf);
            // Instantiate();
            spawned.transform.position = pos;
            Vector3 angle = spawned.transform.localEulerAngles;

            angle.x += angleOffsets.GetRandom();
            spawned.transform.localEulerAngles = angle;
            //spawned.transform.SetParent(tempParent);
            
            PopulateBook(spawned);
        }
    }

    void PopulateBook(GameObject spawned)
    {
        FlashmobBook book = spawned.GetComponent<FlashmobBook>();
        if (book == null){ return; }
        BookStruct bookStruct = booklist.booklist[i];
        book.SetBook(bookStruct);
        i++;
        if (i >= booklist.booklist.Count)
        {
            i = 0;
        }
    }
    #endif
}
