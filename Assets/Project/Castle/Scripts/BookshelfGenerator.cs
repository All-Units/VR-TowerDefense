using System;
using System.Collections.Generic;
using UnityEditor;

using Random = UnityEngine.Random;
using UnityEngine;
using System.Linq;
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
    void Start()
    {
        _loadSeriesData();
    }

#if UNITY_EDITOR
    static bool regeneratingAllShelving = false;
    public void RegenAllShelving()
    {
        _loadSeriesData();
        regeneratingAllShelving = true;
        i = 0;
        var shelves = transform.root.GetComponentsInChildren<BookshelfGenerator>();
        foreach (var gen in shelves)
        {
            //BookshelfGenerator gen = c.GetComponent<BookshelfGenerator>();
            gen.RegenShelves();
        }
        print($"Regened {shelves.Length} shelves");
        regeneratingAllShelving = false;
    }
    Transform tempParent;
    public void RegenShelves()
    {
        _loadSeriesData();
        if (regeneratingAllShelving == false)
        {
            i = 0;
            print("Regen shelving CLICKED");
        }
        
        
        _clearShelves();
        foreach (Transform shelf in booksParent)
            _generateShelf(shelf);
    }
    public static Dictionary<String, SeriesStruct> seriesData = new Dictionary<string, SeriesStruct>();
    void _loadSeriesData()
    {
        // Do nothing if we're already loaded
        if (seriesData.Count > 0) 
            { return; }
        foreach (SeriesStruct s in booklist.seriesData)
        {
            seriesData[s.seriesName] = s;
        }
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

    static int i = 0;
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
            float rolled = Random.Range(0, 100);
            if (skipPercent >= rolled)
            {
                continue;
            }
                
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
