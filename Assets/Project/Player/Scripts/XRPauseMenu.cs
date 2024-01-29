using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

    Transform cam => InventoryManager.instance.playerCameraTransform;


    public static Action OnPause;
    public static Action OnResume;

    #region UnityEvents

    // Start is called before the first frame update
    void Start()
    {
        if (togglePauseButton == null)
        {
            Debug.LogError("Pause menu had no button assigned! Returning");
        }

        togglePauseAction.started += TogglePauseAction_started;
        InitBubbles();
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
    void MainMenu(ActivateEventArgs args)
    {
        StartCoroutine(_QuitRoutine());
    }
    IEnumerator _QuitRoutine()
    {
        FadeScreen.instance.FadeOut();

        yield return new WaitForSeconds(FadeScreen.instance.fadeDuration);
        SceneManager.LoadSceneAsync("MainMenu");
    }
    void Quit(ActivateEventArgs args)
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        return;
#endif
        Application.Quit();
    }
    #endregion
    #region HelperFunctions
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
            if (xr == null) continue;
            if (name == "Main Menu")
            {
                xr.activated.AddListener(MainMenu);
            }
            else if (name == "Quit")
                xr.activated.AddListener(Quit);
        }
        bubbleParent.gameObject.SetActive(false);
    }
    #endregion




}

