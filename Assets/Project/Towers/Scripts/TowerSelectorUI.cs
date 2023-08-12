using System;
using System.Collections;
using System.Collections.Generic;
using Project.Towers.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Quaternion = System.Numerics.Quaternion;

public class TowerSelectorUI : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("To enable/disable player movement")]
    [SerializeField] private DynamicMoveProvider playerMover;
    [SerializeField] private InputActionReference navigateControl;
    [SerializeField] private InputActionReference grip;
    [Header("References")]
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private RectTransform pointerTransform;
    [SerializeField] private GameObject dividingLine;
    [SerializeField] private GameObject towerIconPrefab;
    [SerializeField] private Transform towerIconsParent;
    [SerializeField] private Transform cameraPos;
    
    [SerializeField] private float angleOffset = -45f;
    [Tooltip("How quickly a tap must be released to be considered a tap / hold")]
    [SerializeField] private float toggleTapTime;


    [Header("Cylinder vars")] 
    [SerializeField] private Transform cylinderParent;

    [SerializeField] private float height = 4f;
    [SerializeField] private float distanceFromPlayer = 4f;
    [SerializeField] private float yOffset = -1f;
    [SerializeField] private float towerScale = 0.3f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private List<Tower_SO> towers = new List<Tower_SO>();
    
    [SerializeField]
    private List<TowerIcon> _icons = new List<TowerIcon>();
    private int current_icon_i = -1;
    private void Awake()
    {
        FillCylinder();
        current_icon_i = -1;
        //_icons[current_icon_i].Select();
        selectPanel.SetActive(false);
        StartCoroutine(_delaySelect());
    }

    IEnumerator _delaySelect()
    {
        yield return null;
        yield return null;
        SelectTower(0);
    }

    private void OnEnable()
    {
        grip.action.started += LeftHandGrip;
        grip.action.canceled += LeftHandGrip;
        
    }

    private void OnDisable()
    {
        grip.action.started -= LeftHandGrip;
        grip.action.canceled -= LeftHandGrip;
        OnCloseSelector();
    }
    #region OnOpenCloseSelector
    private float lastOpenTime;
    /// <summary>
    /// Logic when the player changes grip in the left hand
    /// </summary>
    /// <param name="obj"></param>
    void LeftHandGrip(InputAction.CallbackContext obj)
    {
        bool open = selectPanel.activeInHierarchy;
        if (obj.phase == InputActionPhase.Started && open == false)
        {
            lastOpenTime = Time.time;
            OnOpenSelector();
            
        }
        else if (obj.phase == InputActionPhase.Canceled && Time.time - lastOpenTime > toggleTapTime)
        {
            OnCloseSelector();
        }
        else if (obj.phase == InputActionPhase.Started && open)
        {
            OnCloseSelector();
        }
    }
    

    
    /// <summary>
    /// Logic when opening the tower select panel
    /// </summary>
    void OnOpenSelector()
    {
        navigateControl.action.performed += OnLook;
        selectPanel.SetActive(true);
        //Disable player movement
        playerMover.CanMove = false;
        PositionTowerSelector();
    }
    
    void PositionTowerSelector()
    {
        Vector3 pos = cameraPos.position;
        Vector3 dir = cameraPos.forward;
        dir.y = yOffset;
        dir *= distanceFromPlayer;
        pos += dir;
        selectPanel.transform.position = pos;
    }
    
    void OnCloseSelector()
    {
        navigateControl.action.performed -= OnLook;
        selectPanel.SetActive(false);
        //Reenable player movement
        playerMover.CanMove = true;
    }
    #endregion
    public void OnLook(InputAction.CallbackContext obj)
    {
        PointArrow(obj.ReadValue<Vector2>());
    }

    private float _deg;
    /// <summary>
    /// Which index of the towers prefab list is currently selected
    /// </summary>
    private int _i;
    void PointArrow(Vector2 dir)
    {
        //Legacy, based on direction of joystick
        //float degrees = Mathf.Atan2(dir.y, dir.x) * (180f / Mathf.PI);
        
        float degrees = cylinderParent.localEulerAngles.y;

        degrees += (dir.x * rotateSpeed * Time.deltaTime);
        
        cylinderParent.localEulerAngles = new Vector3(0f, degrees, 0f);
        _deg = cylinderParent.localEulerAngles.y;
        _i = (int)((360f - _deg) / arc);
        if (_i != current_icon_i)
        {
            current_icon_i = _i;
            SelectTower(_i);
        }
        
    }

    public void SelectTower(int i)
    {
        //Ignore i's out of bounds (likely from float -> int conversion errors)
        if (i >= _icons.Count || i < 0) return;
        
        _icons[i].Select();
    }
    float arc => (360f / towers.Count);
    

    void FillCylinder()
    {
        for (int i = 0; i < towers.Count; i++)
        {
            GameObject tower = Instantiate(towers[i].iconPrefab, cylinderParent);
            tower.transform.localScale = Vector3.one * towerScale;
            float degrees = arc * i + (arc / 2f);
            tower.transform.localEulerAngles = new Vector3(0f, degrees, 0f);
            //tower.transform.Translate(new Vector3(0f, ((float)i / towers.Count) * height, 0f));
            TowerIcon icon = tower.GetComponentInChildren<TowerIcon>();
            icon.towerSO = towers[i];
            icon.nameText.text = towers[i].name;
            _icons.Add(icon);
        }
    }
    /*2D Tower Select Legacy
     [SerializeField] private List<Sprite> towerPrefabs = new List<Sprite>();
     /// <summary>
    /// Populates the tower select wheel
    /// </summary>
    void FillWheel()
    {
        for (int i = 0; i < towerPrefabs.Count; i++)
        {
            GameObject ico = Instantiate(towerIconPrefab, towerIconsParent);
            
            float degrees = arc * i + (arc / 2f);

            ico.transform.localEulerAngles = new Vector3(0f, 0f, -1f * degrees);
            Image image = ico.GetComponentInChildren<Image>();
            image.sprite = towerPrefabs[i];
            image.transform.localEulerAngles = new Vector3(0f, 0f, degrees);
            _icons.Add(ico.GetComponentInChildren<TowerIcon>());
            if (i != 0)
            {
                GameObject line = Instantiate(dividingLine, dividingLine.transform.parent);
                line.transform.localEulerAngles = new Vector3(0f, 0f, arc * i);
            }
        }
    }
    
    
        pointerTransform.localEulerAngles = new Vector3(0f, 0f, degrees);
        deg = pointerTransform.localEulerAngles.z;
        i = (int)((360f - deg) / arc);
        if (i != current_icon_i)
        {
            current_icon_i = i;
            SelectTower(i);
        }
     */
}
