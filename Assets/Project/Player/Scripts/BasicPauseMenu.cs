using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BasicPauseMenu : MonoBehaviour
{
    public GameObject circleFillPanel;

    public Image fillCircleImage;

    public GameObject pausePanel;
    
    [SerializeField] private float openPauseTime = 0.5f;
    
    

    public static BasicPauseMenu instance;
    public bool IsSecondaryHeld = false;


    private void Awake()
    {
        instance = this;
        CloseAll();
    }

    

    public void Quit()
    {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    public void QuitToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(0);
    }

    void CloseAll()
    {
        circleFillPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    void Pause()
    {
        pausePanel.SetActive(true);
        circleFillPanel.SetActive(false);
        Time.timeScale = 0f;
    }
    public void Resume()
    {
        Time.timeScale = 1f;
        CloseAll();
    }

    public static void StartPauseFill()
    {
        instance.StartCoroutine(instance._openPauseCircle());
    }
    IEnumerator _openPauseCircle()
    {
        if (Time.timeScale == 0f)
        {
            Resume();
            yield break;
        }
        float t = 0f;
        circleFillPanel.SetActive(true);
        fillCircleImage.fillAmount = 0f;
        //Fill the circle until it's full, or the button is released
        while (t < openPauseTime && IsSecondaryHeld)
        {
            t += Time.deltaTime;
            float fill = Mathf.Lerp(0, 1, t / openPauseTime);
            fillCircleImage.fillAmount = fill;
            yield return null;
        }
        circleFillPanel.SetActive(false);
        //The inventory is to be opened
        if (t >= openPauseTime)
        {
            //If the inventory is closed, open it
            if (pausePanel.activeInHierarchy == false)
                Pause();
            else Resume();
        }

        fillCircleImage.fillAmount = 0f;

    }
}
