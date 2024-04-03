using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class XRPauseMenu : MonoBehaviour
{
    [SerializeField] float distanceFromPlayer;
    [SerializeField] float heightOffset = -0.5f;
    [SerializeField] float arcOffset;

    [SerializeField] GameObject bubblePrefab;
    
    [SerializeField] Transform bubbleParent;
    [SerializeField] InputActionReference togglePauseButton;
    InputAction togglePauseAction => Utilities.GetInputAction(togglePauseButton);

    public static bool IsPaused;
    bool isPaused = false;

    [Header("Settings Panel")]
    [SerializeField] GameObject settingsPanelPrefab;
    [SerializeField] float settingsDistance = 5f;
    [SerializeField] float settingsHeight = 2f;

    [SerializeField] Sprite settingsIcon;
    [SerializeField] Color settingsColor;
    [SerializeField] Sprite mainMenuIcon;
    [SerializeField] Color mainMenuColor;
    [SerializeField] Sprite quitIcon;
    [SerializeField] Color quitColor;

    Transform cam { 
        get { 
            if (InventoryManager.instance != null)
                return InventoryManager.instance.playerCameraTransform;
            if (_camera == null)
                _camera = GetComponentInChildren<Camera>();
            return _camera.transform;
        }
    }
    Camera _camera;

    public static Action OnPause;
    public static Action OnResume;

    #region UnityEvents
    private void Awake()
    {
        VolumeManager.InitValuesFromCache();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (togglePauseButton == null)
        {
            Debug.LogError("Pause menu had no button assigned! Returning");
        }

        togglePauseAction.started += TogglePauseAction_started;
        InitBubbles();
        _ResumeFlag();
        OnPause += _PauseFlag;
        OnResume += _ResumeFlag;

        OnResume += _TurnOffSettings;
        VolumeManager.InitValuesFromCache();
    }
    private void OnDestroy()
    {
        OnPause -= _PauseFlag;
        OnResume -= _ResumeFlag;
        OnResume -= _TurnOffSettings;

        togglePauseAction.started -= TogglePauseAction_started;
    }


    private void TogglePauseAction_started(InputAction.CallbackContext obj)
    {
        isPaused = !isPaused;
        bubbleParent.gameObject.SetActive( isPaused );
        if (isPaused)
            OnPause?.Invoke();
        else
            OnResume?.Invoke();
        RepositionBubbles();
        IsPaused = isPaused;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region PauseMenuFunctions
    GameObject _settingsPanel = null;
    void SettingsPressed(ActivateEventArgs args)
    {
        //Toggle settings panel
        if (_settingsPanel != null)
        {
            //If it's on, turn it off
            //Or vice versa
            _settingsPanel.SetActive(!_settingsPanel.activeInHierarchy);
        }
        _InitSettingsPanel();
        _RepositionSettings();
    }
    void NextRound(ActivateEventArgs args)
    {
        EnemyManager.SkipToNextRound = true;
        TogglePauseAction_started(new InputAction.CallbackContext());
    }
    void MainMenu(ActivateEventArgs args)
    {
        
        StartCoroutine(_QuitRoutine());
    }
    IEnumerator _QuitRoutine()
    {
        FadeScreen.instance.FadeOut();

        yield return new WaitForSeconds(FadeScreen.instance.fadeDuration);
        IsPaused = false;
        isPaused = false;
        SceneManager.LoadSceneAsync("MainMenu");
    }
    public void Quit(ActivateEventArgs args)
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        return;
#endif
        Application.Quit();
    }
    #endregion
    #region HelperFunctions
    void _InitSettingsPanel()
    {
        if (_settingsPanel != null) return;
        
        _settingsPanel = new GameObject();
        _settingsPanel.name = "Settings Panel";
        _settingsPanel.transform.position = bubbleParent.transform.position;
        _settingsPanel.transform.parent = bubbleParent.transform;
        GameObject panel = Instantiate(settingsPanelPrefab, _settingsPanel.transform);
        panel.transform.localPosition = Vector3.zero;

        panel.transform.localEulerAngles = Vector3.zero;
        Vector3 dir = panel.transform.forward * settingsDistance;
        dir += new Vector3(0f, settingsHeight, 0f);
        panel.transform.localPosition += dir;
        string[] sliderTypes = new[] { "master", "soundtrack", "sfx" };
        foreach (Slider slider in panel.GetComponentsInChildren<Slider>())
        {
            foreach (string type in sliderTypes)
            {

                //The slider needs to load the value from cache
                if (slider.gameObject.name.Contains(type))
                {
                    slider.value = PlayerPrefs.GetFloat(type, 1f);
                    slider.onValueChanged.Invoke(slider.value);
                }
            }
        }


    }

    void _RepositionSettings()
    {
        _settingsPanel.transform.position = cam.transform.position;
        Vector3 angle = new Vector3(0f, cam.transform.eulerAngles.y, 0f);
        _settingsPanel.transform.eulerAngles = angle;
        
    }
    int bubbleCount = 3;
    void RepositionBubbles()
    {
        float offset = ((float)(bubbleCount - 1) * arcOffset) * -1f;

        float angle = cam.eulerAngles.y + offset / 2f;
        bubbleParent.eulerAngles = new Vector3(0f, angle, 0f);
        //print($"Repositioned bubbles: cam was {cam.eulerAngles.y}°, offset by {offset / 2f}° for a total of {angle}°");
        bubbleParent.position = cam.position;
        bubbleParent.Translate(new Vector3(0f, heightOffset, 0f));
    }
    string[] bubbleNames = new string[] { "Settings", "Main Menu", "Quit"};
    void InitBubbles()
    {
        bubbleParent.parent = null;
        float offset = ((float)bubbleCount * arcOffset) * -1f;
        float angle = cam.eulerAngles.y + offset / 2f;
        bubbleParent.eulerAngles = new Vector3(0f, angle, 0f);
        int i = 0;
        foreach (string name in bubbleNames)
        {
            GameObject bp = new GameObject();
            bp.name = $"Bubble {i + 1}";
            bp.transform.parent = bubbleParent;
            bp.transform.localPosition = Vector3.zero;
            GameObject bubble = Instantiate(bubblePrefab, bp.transform);
            angle = arcOffset * i;
            bp.transform.localEulerAngles = new Vector3(0f, angle, 0f);
            bubble.transform.localPosition = new Vector3(0f, 0f, distanceFromPlayer);
            i++;
            var bubbleMenu = bp.GetComponentInChildren<BubbleMenuOption>();
            if (bubbleMenu != null ) Destroy(bubbleMenu);
            var text = bp.GetComponentInChildren<TMP_Text>();
            if (text)
                text.text = name;
            else
                print("No text!!!");

            XRSimpleInteractable xr = bubble.GetComponentInChildren<XRSimpleInteractable>();
            SpriteRenderer renderer = xr.GetComponentInChildren<SpriteRenderer>();
            if (xr == null) continue;
            if (name == "Main Menu")
            {
                xr.activated.AddListener(MainMenu);
                _SetSprite(renderer, mainMenuIcon, mainMenuColor);
            }
            else if (name == "Quit") {
                xr.activated.AddListener(Quit);
                _SetSprite(renderer, quitIcon, quitColor);
            }
            
            else if (name == "Settings") { 
                xr.activated.AddListener(SettingsPressed);
                _SetSprite(renderer, settingsIcon, settingsColor);
            }
                
        }
        bubbleParent.gameObject.SetActive(false);
    }
    void _SetSprite(SpriteRenderer renderer, Sprite sprite, Color c)
    {
        if (renderer == null) return;
        renderer.sprite = sprite;
        renderer.color = c;
    }
    void _PauseFlag()
    {
        Material mat = Resources.Load<Material>("Materials/ShaderTest");
        mat.SetInt("_Playing", 0);
    }
    void _ResumeFlag()
    {
        Material mat = Resources.Load<Material>("Materials/ShaderTest");
        mat.SetInt("_Playing", 1);
    }
    void _TurnOffSettings()
    {
        if (_settingsPanel == null) return;
        _settingsPanel.SetActive(false);
    }
    #endregion




}

