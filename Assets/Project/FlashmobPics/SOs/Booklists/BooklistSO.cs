using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/BooklistSO", fileName = "Booklist")]
public class BooklistSO : ScriptableObject
{
    [SerializeField]
    public List<BookStruct> booklist = new List<BookStruct>();    
}
[System.Serializable]
public struct BookStruct
    {
        public string name => title;
        public string title;
        public string author;
        public string series;
        public string bookDescription;
        public Sprite cover;
        public Color coverColor;
        public bool DisplayBookDetails;
    }
