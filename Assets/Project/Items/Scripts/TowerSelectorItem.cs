using System;
using System.Collections;
using System.Collections.Generic;
using Project.Towers.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Quaternion = System.Numerics.Quaternion;

public class TowerSelectorItem : BaseItem
{
    [Header("Input")]
    [Tooltip("To enable/disable player movement")]
    [SerializeField] private DynamicMoveProvider playerMover;
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
    [SerializeField] private float timeToSnap = 1f;
    [SerializeField] private int snapScale = 90;
    [SerializeField] private List<Tower_SO> towers = new List<Tower_SO>();
    
    [SerializeField]
    private List<TowerIcon> _icons = new List<TowerIcon>();
    private int current_icon_i = -1;
    public static TowerSelectorItem instance;

    private void Awake()
    {
        base.Awake();
        instance = this;
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

    private bool inittedInv = false;
    private void OnEnable()
    {
        //Do nothing until we have an inventory
        if (_inventory == null)return;
        inittedInv = true;
        grip.action.started += LeftHandGrip;
        grip.action.canceled += LeftHandGrip;
        
    }

    private void OnDisable()
    {
        OnCloseSelector(false);
        if (inittedInv == false) return;
        
        grip.action.started -= LeftHandGrip;
        grip.action.canceled -= LeftHandGrip;
        
    }
    #region OnOpenCloseSelector
    private float lastOpenTime;
    private float lastGripTime;
    /// <summary>
    /// Logic when the player changes grip in the left hand
    /// </summary>
    /// <param name="obj"></param>
    void LeftHandGrip(InputAction.CallbackContext obj)
    {
        if (Time.time - lastGripTime <= toggleTapTime)
        {
            return;
        }

        lastGripTime = Time.time;
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
        stick.action.performed += OnLook;
        selectPanel.SetActive(true);
        //Add a movement lock
        base._inventory.PlaceLock(gameObject);
        
        UpdateAllTowers();
    }
    
    
    void OnCloseSelector(bool removeLock = true)
    {
        if (inittedInv)
            stick.action.performed -= OnLook;
        selectPanel.SetActive(false);
        //Reenable player movement, if it was enabled before
        if (removeLock)
            base._inventory.RemoveLock(gameObject);


    }
    #endregion
    public void OnLook(InputAction.CallbackContext obj)
    {
        Vector2 dir = obj.ReadValue<Vector2>().normalized;
        PointArrow(dir);
    }

    private float _deg;
    private bool isSnapping = false;
    /// <summary>
    /// Which index of the towers prefab list is currently selected
    /// </summary>
    private int _i;
    void PointArrow(Vector2 dir)
    {
        if (_inventory.IsOpen)
            return;
        //Legacy, based on direction of joystick
        //float degrees = Mathf.Atan2(dir.y, dir.x) * (180f / Mathf.PI);
        
        float degrees = cylinderParent.localEulerAngles.y;

        degrees += (dir.x * rotateSpeed * Time.deltaTime);
        //If there is input on the y axis, and we haven't snapped recently
        if (Math.Abs(dir.y) >= 0.9 && isSnapping == false)
        {
            StartCoroutine(snapSelector(Math.Sign(dir.y)));
            return;
        }
        cylinderParent.localEulerAngles = new Vector3(0f, degrees, 0f);
        _select_i();
        
    }

    IEnumerator snapSelector(int ySign)
    {
        float _waitedTime = 0f;
        var startDeg = cylinderParent.localEulerAngles;
        snapScale = (int)arc;
        while (_waitedTime <= timeToSnap)
        {
            //Our target is the snap scale past our current angle, in the direction of input
            float end = startDeg.y + (snapScale * ySign);
            
            //We're lerping between our start angle, and the target
            float deg = Mathf.LerpAngle(startDeg.y, end, _waitedTime / timeToSnap);
            Vector3 angle = startDeg;
            angle.y = deg;
            cylinderParent.localEulerAngles = angle;
            yield return null;
            _waitedTime += Time.deltaTime;
        }
        _select_i();
        isSnapping = false;

    }

    void _select_i()
    {
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
        TowerSpawnManager.RefreshGhost();
    }
    float arc => (360f / towers.Count);
    

    void FillCylinder()
    {
        for (int i = 0; i < towers.Count; i++)
        {
            Tower_SO t = towers[i];
            GameObject tower = Instantiate(t.iconPrefab, cylinderParent);
            tower.transform.localScale = Vector3.one * towerScale;
            float degrees = arc * i + (arc / 2f);
            tower.transform.localEulerAngles = new Vector3(0f, degrees, 0f);
            //tower.transform.Translate(new Vector3(0f, ((float)i / towers.Count) * height, 0f));
            TowerIcon icon = tower.GetComponentInChildren<TowerIcon>();
            icon.towerSO = t;
            icon.nameText.text = t.name;
            icon.descriptionText.text = $"Cost: <color=green>{t.cost}gp</color>\n" +
                                        $"{t.description}";
            _icons.Add(icon);
        }
    }

    public static void DelayUpdateTowers()
    {
        instance.StartCoroutine(instance.delayUpdate());
    }
    IEnumerator delayUpdate()
    {
        yield return null;
        UpdateAllTowers();
    }
    public static void UpdateAllTowers()
    {
        foreach (TowerIcon tower in instance._icons)
        {
            tower.SetCanAfford();
        }
    }
    /*/// <summary>
          /// 2D Tower Select Legacy
          /// </summary>
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
