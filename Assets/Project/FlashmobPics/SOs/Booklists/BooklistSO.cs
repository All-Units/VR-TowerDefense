using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/BooklistSO", fileName = "Booklist")]
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
        public string bookDescription;
        public Texture2D cover;
        public Color coverColor;
        public bool DisplayBookDetails;
    }
}
