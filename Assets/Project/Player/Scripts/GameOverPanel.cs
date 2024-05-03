using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Image = UnityEngine.UI.Image;

public class GameOverPanel : MonoBehaviour
{
    public List<StatTracker> stats = new();
    [FormerlySerializedAs("_distanceFromPlayer")]
    [Header("Gameplay variables")]
    [Range(0f, 7f)]
    [SerializeField]
    private float distanceFromPlayer = 3f;
    [FormerlySerializedAs("_height")]
    [Range(-1f, 4f)]
    [SerializeField]
    private float height = 0.5f;
    [SerializeField] private float _freeMoveRecenterThreshold = 5f;
    [FormerlySerializedAs("_winString")] [SerializeField]
    private string winString = "VICTORY";
    [FormerlySerializedAs("_loseString")] [SerializeField]
    private string loseString = "DEFEAT";
    [FormerlySerializedAs("_winColor")] [SerializeField]
    private Color winColor = Color.green;
    [FormerlySerializedAs("_loseColor")] [SerializeField]
    private Color loseColor = Color.red;
    [FormerlySerializedAs("_winSprite")] [SerializeField]
    private Sprite winSprite = null;
    [FormerlySerializedAs("_loseSprite")] [SerializeField]
    private Sprite loseSprite = null;
     
    [Header("Obj references")]
    [SerializeField]
    private Transform canvasTransform;
    [FormerlySerializedAs("WinLoseLabel")] [SerializeField]
    private TextMeshProUGUI winLoseLabel;
    [FormerlySerializedAs("WinLoseIcon")] [SerializeField]
    private Image winLoseIcon;
    [FormerlySerializedAs("WinLoseBanner")] [SerializeField]
    private Image winLoseBanner;
    [FormerlySerializedAs("_contentParent")] [SerializeField]
    private Transform contentParent;
    [SerializeField] private GameObject statTextPrefab;
    [SerializeField] private InputActionReference moveInput;
    private InputAction input => Utilities.GetInputAction(moveInput);
    
    // Start is called before the first frame update
    private void Start()
    {
        if (canvasTransform == null) canvasTransform = transform.GetChild(0);
        canvasTransform.gameObject.SetActive(false);
        GameStateManager.onGameWin += OnGameWin;
        GameStateManager.onGameLose += OnGameLose;
    }

    private void OnDestroy()
    {
        GameStateManager.onGameWin -= OnGameWin;
        GameStateManager.onGameLose -= OnGameLose;
        if (moveInput != null)
        {
            input.started -= Input_started;
            input.canceled -= Input_canceled;
        }
    }

    private void UpdateStats()
    {
        contentParent.DestroyChildren();

        foreach (var stat in stats)
        {
            var count = stat.total - stat.getSerializeValue;
            if(count <= 0) continue;
            
            var prefab = Instantiate(statTextPrefab, contentParent);
            prefab.SetActive(true);
            var text = prefab.GetComponentInChildren<TextMeshProUGUI>();
            text.text = $"{stat.displayName} {stat.statName} : {count}";
        }
    }

    private void OnGameWin()
    {
        OnGameEnd(winString, winColor, winSprite);
    }

    private void OnGameLose()
    {
        OnGameEnd(loseString, loseColor, loseSprite);
    }
    public void TestWin()
    {
        OnGameWin();
    }

    private void OnGameEnd(string endString, Color endColor, Sprite sprite)
    {
        if (moveInput != null)
        {
            input.started += Input_started;
            input.canceled += Input_canceled;
        }

        PlayerStateController.teleporter.endLocomotion += _RepositionAfterTeleport;
        Debug.Log("Displaying end panel!");
        UpdateStats();
        canvasTransform.gameObject.SetActive(true);
        _RepositionPanel();
        winLoseBanner.color = endColor;
        winLoseLabel.text = endString;

        winLoseIcon.sprite = sprite;
    }

    private void _RepositionAfterTeleport(LocomotionSystem system)
    {
        _RepositionAfter();
    }

    private void _RepositionAfter(float t = 0.1f)
    {
        StartCoroutine(_RepositionAfterRoutine(t));
    }

    private IEnumerator _RepositionAfterRoutine(float t)
    {
        yield return new WaitForSeconds(t);
        _RepositionPanel();
    }
    private void Input_canceled(InputAction.CallbackContext obj)
    {
        if (DynamicMoveProvider.canMove == false) return;
        _moveHeld = false;
    }

    private bool _moveHeld = false;
    private void Input_started(InputAction.CallbackContext obj)
    {
        if (DynamicMoveProvider.canMove == false) return;
        _moveHeld = true;
        StartCoroutine(_TrackMovement());
    }

    private IEnumerator _currentMoveTracker = null;
    private Transform cam => InventoryManager.instance.playerCameraTransform;

    private IEnumerator _TrackMovement()
    {
        Vector3 _lastPos = cam.position; _lastPos.y = 0f;
        float distance = 0f;
        while (_moveHeld)
        {
            yield return null;
            Vector3 pos = cam.position; pos.y = 0f;
            distance += Vector3.Distance(_lastPos, pos);
            if (distance >= _freeMoveRecenterThreshold)
            {
                _RepositionAfter();
                distance = 0f;
            }
            _lastPos = pos;
        }
    }

    private void _RepositionPanel()
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
            //print($"Hit y level {hit.point.y}, canvas currently at {canvasPos.y}");
            if (hit.point.y > canvasPos.y)
            {
                float offset = Mathf.Max(height, 0.3f);
                canvasPos.y = hit.point.y + offset;
                canvasTransform.position = canvasPos;
                //print($"Canvas was too low, moving up to {canvasTransform.position.y}");
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
}
