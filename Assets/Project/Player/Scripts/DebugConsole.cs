using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    public static DebugConsole instance;
    [SerializeField] private GameObject background;
    [SerializeField] private Transform textParents;
    [SerializeField] private TextMeshProUGUI consoleText;

    [SerializeField] private float displayTime = 2f;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        consoleText.text = "";
        background.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void print(string s)
    {
        instance.background.SetActive(true);
        GameObject go = Instantiate(instance.consoleText.gameObject, instance.textParents);
        
        go.SetActive(true);
        go.GetComponent<TextMeshProUGUI>().text = s;
        Destroy(go, instance.displayTime);
        instance.StartCoroutine(instance.CheckToDeactivate());
    }

    IEnumerator CheckToDeactivate()
    {
        yield return new WaitForSeconds(displayTime + 0.1f);
        if (textParents.childCount == 0)
            background.SetActive(false);
    }
}
