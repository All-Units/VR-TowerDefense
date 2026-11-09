using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "SO/BooklistSO", fileName = "Booklist")]
[ExecuteInEditMode]
public class BooklistSO : ScriptableObject
{
    [SerializeField]
    public List<Book> booklist = new List<Book>();
    [System.Serializable]
    public struct Book
    {
        public string name => title;
        public string title;
        public string author;
        public Texture2D cover;
    }
    private void Update()
    {
        Debug.Log("updating booklist");
    }
}
