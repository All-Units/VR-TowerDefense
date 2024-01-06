using System;
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

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
            var shelves = g.transform.root.GetComponentsInChildren<BookshelfGenerator>();
            foreach (var gen in shelves)
            {
                //BookshelfGenerator gen = c.GetComponent<BookshelfGenerator>();
                gen.RegenShelves();
            }
            
        }
        base.OnInspectorGUI();
    }
}
public class BookshelfGenerator : MonoBehaviour
{
    public Transform booksParent;
    public Transform rightBound;
    public List<Book> bookPrefabs = new List<Book>(4);
    public int skipPercent = 40;
    [Tooltip("Lower and upper bounds for distance between books")]
    public Vector2 lerpRange = new Vector2(.1f, .2f);
    [SerializeField] private TextAsset booklist;

    private List<string> lines = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RegenShelves()
    {
        parseBooklist();
        _clearShelves();
        foreach (Transform shelf in booksParent)
            _generateShelf(shelf);
    }

    void _clearShelves()
    {
        foreach (Transform shelf in booksParent)
        {
            foreach (Transform book in shelf)
            {
                DestroyImmediate(book.gameObject);
            }
        }
    }

    private List<int> angleOffsets = new List<int>() { -10, 0, 0, 0, 10 };
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
            GameObject spawned = Instantiate(bookPrefabs.GetRandom().gameObject, shelf);
            spawned.transform.position = pos;
            Vector3 angle = spawned.transform.localEulerAngles;

            angle.z += angleOffsets.GetRandom();
            spawned.transform.localEulerAngles = angle;
            
            PopulateBook(spawned);
        }
    }

    void PopulateBook(GameObject prefab)
    {
        var book = prefab.GetComponent<Book>();
        string line = lines.GetRandom();
        var split = line.Split("|");
        try
        {
        book.title = split[0].Trim();
        book.author = split[1].Trim();
        book.description = split[2].Trim();
        }
        catch (Exception e)
        {
            print($"{line} threw exception when split");
            throw;
        }
        
        book.populateCover();
    }

    void parseBooklist()
    {
        lines = new List<string>();
        foreach (string line in booklist.text.Split("\n"))
        {
            if (line == "") continue;
            lines.Add(line);
        }
    }
}
#endif