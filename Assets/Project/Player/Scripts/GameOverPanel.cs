using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Image = UnityEngine.UI.Image;

public class GameOverPanel : MonoBehaviour
{
    public List<StatTracker> stats = new();
    [FormerlySerializedAs("_distanceFromPlayer")]
    [Header("Gameplay variables")]
    [Range(0f, 7f)]
    [SerializeField] float distanceFromPlayer = 3f;
    [FormerlySerializedAs("_height")]
    [Range(-1f, 4f)]
    [SerializeField] float height = 0.5f;

    [FormerlySerializedAs("_winString")] [SerializeField] string winString = "VICTORY";
    [FormerlySerializedAs("_loseString")] [SerializeField] string loseString = "DEFEAT";
    [FormerlySerializedAs("_winColor")] [SerializeField] Color winColor = Color.green;
    [FormerlySerializedAs("_loseColor")] [SerializeField] Color loseColor = Color.red;
    [FormerlySerializedAs("_winSprite")] [SerializeField] Sprite winSprite = null;
    [FormerlySerializedAs("_loseSprite")] [SerializeField] Sprite loseSprite = null;
     
    [Header("Obj references")]
    [SerializeField] Transform canvasTransform;
    [FormerlySerializedAs("WinLoseLabel")] [SerializeField] TextMeshProUGUI winLoseLabel;
    [FormerlySerializedAs("WinLoseIcon")] [SerializeField] Image winLoseIcon;
    [FormerlySerializedAs("WinLoseBanner")] [SerializeField] Image winLoseBanner;
    [FormerlySerializedAs("_contentParent")] [SerializeField] Transform contentParent;
    [SerializeField] GameObject statTextPrefab;
    
    Dictionary<StatTracker, int> _startValues = new();
    
    // Start is called before the first frame update
    void Start()
    {
        if (canvasTransform == null) canvasTransform = transform.GetChild(0);
        canvasTransform.gameObject.SetActive(false);
        GameStateManager.onGameWin += OnGameWin;
        GameStateManager.onGameLose += OnGameLose;
        foreach (var stat in stats)
            _startValues[stat] = stat.getSerializeValue;
    }

    private void OnDestroy()
    {
        GameStateManager.onGameWin -= OnGameWin;
        GameStateManager.onGameLose -= OnGameLose;
    }

    void UpdateStats()
    {
        contentParent.DestroyChildren();

        foreach (var stat in stats)
        {
            int count = stat.total - _startValues[stat];

            if(count == 0) continue;
            GameObject prefab = Instantiate(statTextPrefab, contentParent);
            prefab.SetActive(true);
            TextMeshProUGUI text = prefab.GetComponentInChildren<TextMeshProUGUI>();
            text.text = $"{stat.displayName} {stat.statName} : {count}";
        }
    }

    void OnGameWin()
    {
        OnGameEnd(winString, winColor, winSprite);
    }
    void OnGameLose()
    {
        OnGameEnd(loseString, loseColor, loseSprite);
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
        winLoseBanner.color = endColor;
        winLoseLabel.text = endString;

        winLoseIcon.sprite = sprite;
    }
    void _RepositionPanel()
    {
        if (InventoryManager.instance == null) return;
        Transform cam = InventoryManager.instance.playerCameraTransform;

        transform.position = cam.position;

        Vector3 angle = new Vector3(0f, cam.eulerAngles.y - 90f, 0f);
        transform.eulerAngles = angle;
        canvasTransform.localPosition = new Vector3(distanceFromPlayer, height, 0f);

        Vector3 canvasPos = canvasTransform.position;
        Vector3 center = canvasPos + Vector3.up * 100f;
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Ground");
        if (Physics.Raycast(center, Vector3.down, out hit, float.PositiveInfinity, mask))
        {
            print($"Hit y level {hit.point.y}, canvas currently at {canvasPos.y}");
            if (hit.point.y > canvasPos.y)
            {
                float offset = Mathf.Max(height, 0.3f);
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
