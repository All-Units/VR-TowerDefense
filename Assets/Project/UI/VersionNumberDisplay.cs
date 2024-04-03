using TMPro;
using UnityEngine;

public class VersionNumberDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    
    // Start is called before the first frame update
    void Start()
    {
        text.text = Application.version;
    }
}
