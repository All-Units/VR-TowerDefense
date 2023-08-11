using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class TowerSelectorUI : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private DynamicMoveProvider playerMover;
    [SerializeField] private InputActionReference navigateControl;
    [SerializeField] private InputActionReference grip;
    [Header("References")]
    [SerializeField] private GameObject selectPanel;
    [SerializeField] private RectTransform pointerTransform;
    [SerializeField] private GameObject dividingLine;
    [SerializeField] private GameObject towerIconPrefab;
    [SerializeField] private Transform towerIconsParent;
    [SerializeField] private List<Sprite> towerPrefabs = new List<Sprite>();
    [SerializeField] private float angleOffset = -45f;
    [Tooltip("How quickly a tap must be released to be considered a tap / hold")]
    [SerializeField] private float toggleTapTime;


    [Header("Cylinder vars")] 
    [SerializeField] private Transform cylinderParent;

    [SerializeField] private float height = 4f;
    [SerializeField] private float radius = 3f;
    [SerializeField] private float towerScale = 0.3f;
    [SerializeField] private List<GameObject> towers = new List<GameObject>();
    
    [SerializeField]
    private List<TowerIcon> _icons = new List<TowerIcon>();
    private int current_icon_i = 0;
    private void Awake()
    {
        FillCylinder();
        _icons[current_icon_i].Select();
        selectPanel.SetActive(false);
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
    }
    
    void OnCloseSelector()
    {
        navigateControl.action.performed -= OnLook;
        selectPanel.SetActive(false);
        //Reenable player movement
        playerMover.CanMove = true;
    }
    public void OnLook(InputAction.CallbackContext obj)
    {
        PointArrow(obj.ReadValue<Vector2>());
    }

    public float deg;
    public int i;
    void PointArrow(Vector2 dir)
    {
        float degrees = Mathf.Atan2(dir.y, dir.x) * (180f / Mathf.PI);
        
        degrees += angleOffset;
        cylinderParent.localEulerAngles = new Vector3(0f, degrees, 0f);
        deg = cylinderParent.localEulerAngles.y;
        i = (int)((360f - deg) / arc);
        if (i != current_icon_i)
        {
            current_icon_i = i;
            SelectTower(i);
        }
        /*
        pointerTransform.localEulerAngles = new Vector3(0f, 0f, degrees);
        deg = pointerTransform.localEulerAngles.z;
        i = (int)((360f - deg) / arc);
        if (i != current_icon_i)
        {
            current_icon_i = i;
            SelectTower(i);
        }*/
    }

    public void SelectTower(int i)
    {
        //Ignore i's out of bounds (likely from float -> int conversion errors)
        if (i >= _icons.Count || i < 0) return;
        
        _icons[i].Select();
    }
    float arc => (360f / towers.Count);
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

    void FillCylinder()
    {
        for (int i = 0; i < towers.Count; i++)
        {
            GameObject tower = Instantiate(towers[i], cylinderParent);
            tower.transform.localScale = Vector3.one * towerScale;
            float degrees = arc * i + (arc / 2f);
            tower.transform.localEulerAngles = new Vector3(0f, degrees, 0f);
            //tower.transform.Translate(new Vector3(0f, ((float)i / towers.Count) * height, 0f));
            _icons.Add(tower.GetComponentInChildren<TowerIcon>());
        }
    }
}
