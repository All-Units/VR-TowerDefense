using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class GameOverPanel : MonoBehaviour
{
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
    // Start is called before the first frame update
    void Start()
    {
        if (canvasTransform == null) canvasTransform = transform.GetChild(0);
        canvasTransform.gameObject.SetActive(false);
        GameStateManager.instance.OnGameWin += OnGameWin;
        GameStateManager.instance.OnGameLose += OnGameLose;
    }
    void OnGameWin()
    {
        OnGameEnd(_winString, _winColor, _winSprite);
    }
    void OnGameLose()
    {
        OnGameEnd(_loseString, _loseColor, _loseSprite);
    }
    void OnGameEnd(string endString, Color endColor, Sprite sprite)
    {

        canvasTransform.gameObject.SetActive(true);
        _RepositionPanel();
        WinLoseLabel.color = endColor;
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
            if (hit.point.y > canvasPos.y)
            {
                canvasPos.y = hit.point.y + _height;
                canvasTransform.position = canvasPos;
                print("Canvas was too low, moving up");
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
