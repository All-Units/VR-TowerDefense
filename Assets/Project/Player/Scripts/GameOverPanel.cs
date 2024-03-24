using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class GameOverPanel : MonoBehaviour
{
    public List<StatTracker> stats = new List<StatTracker>();
    [Header("Gameplay variables")]
    [Range(0f, 7f)]
    [SerializeField] float _distanceFromPlayer = 3f;
    [Range(-1f, 4f)]
    [SerializeField] float _height = 0.5f;

    [SerializeField] string _winString = "VICTORY";
    [SerializeField] string _loseString = "DEFEAT";
    [SerializeField] Color _winColor = Color.green;
    [SerializeField] Color _loseColor = Color.red;
    [SerializeField] Sprite _winSprite = null;
    [SerializeField] Sprite _loseSprite = null;
     
    [Header("Obj references")]
    [SerializeField] Transform canvasTransform;
    [SerializeField] TextMeshProUGUI WinLoseLabel;
    [SerializeField] Image WinLoseIcon;
    [SerializeField] Image WinLoseBanner;
    [SerializeField] Transform _contentParent;
    [SerializeField] GameObject statTextPrefab;


    Dictionary<StatTracker, int> startValues = new Dictionary<StatTracker, int>();
    // Start is called before the first frame update
    void Start()
    {
        if (canvasTransform == null) canvasTransform = transform.GetChild(0);
        canvasTransform.gameObject.SetActive(false);
        GameStateManager.OnGameWin += OnGameWin;
        GameStateManager.OnGameLose += OnGameLose;
        foreach (var stat in stats)
            startValues[stat] = stat.getSerializeValue;
    }

    private void OnDestroy()
    {
        GameStateManager.OnGameWin -= OnGameWin;
        GameStateManager.OnGameLose -= OnGameLose;
    }

    void UpdateStats()
    {
        _contentParent.DestroyChildren();

        foreach (var stat in stats)
        {
            if(stat.total == 0) continue;
            GameObject prefab = Instantiate(statTextPrefab, _contentParent);
            prefab.SetActive(true);
            TextMeshProUGUI text = prefab.GetComponentInChildren<TextMeshProUGUI>();
            int count = stat.total - startValues[stat];
            text.text = $"{stat.displayName} {stat.statName} : {count}";
        }
    }

    void OnGameWin()
    {
        OnGameEnd(_winString, _winColor, _winSprite);
    }
    void OnGameLose()
    {
        OnGameEnd(_loseString, _loseColor, _loseSprite);
    }
    public void TestWin()
    {
        OnGameWin();
    }
    void OnGameEnd(string endString, Color endColor, Sprite sprite)
    {
        Debug.Log("Displaying end panel!");
        UpdateStats();
        canvasTransform.gameObject.SetActive(true);
        _RepositionPanel();
        WinLoseBanner.color = endColor;
        WinLoseLabel.text = endString;

        WinLoseIcon.sprite = sprite;
    }
    void _RepositionPanel()
    {
        if (InventoryManager.instance == null) return;
        Transform cam = InventoryManager.instance.playerCameraTransform;

        transform.position = cam.position;

        Vector3 angle = new Vector3(0f, cam.eulerAngles.y - 90f, 0f);
        transform.eulerAngles = angle;
        canvasTransform.localPosition = new Vector3(_distanceFromPlayer, _height, 0f);

        Vector3 canvasPos = canvasTransform.position;
        Vector3 center = canvasPos + Vector3.up * 100f;
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Ground");
        if (Physics.Raycast(center, Vector3.down, out hit, float.PositiveInfinity, mask))
        {
            print($"Hit y level {hit.point.y}, canvas currently at {canvasPos.y}");
            if (hit.point.y > canvasPos.y)
            {
                float offset = Mathf.Max(_height, 0.3f);
                canvasPos.y = hit.point.y + offset;
                canvasTransform.position = canvasPos;
                print($"Canvas was too low, moving up to {canvasTransform.position.y}");
            }
        }
    }

    public void Quit()
    {
        GameStateManager.instance.Quit();
    }
    public void ReturnToMenu()
    {
        GameStateManager.instance.ReturnToMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
