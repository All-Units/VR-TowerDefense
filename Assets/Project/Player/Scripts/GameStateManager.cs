using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;

    [Header("Public variables")] [SerializeField]
    private float waitBeforeEndingTime = 6f;

    [Header("References")]
    [SerializeField] private GameObject YouWinPanel;
    [SerializeField] private GameObject YouLosePanel;

    private void Awake()
    {
        instance = this;
    }

    public static void LoseGame()
    {
        instance._StartEndgame(instance.YouLosePanel);
    }

    public void _StartEndgame(GameObject panel)
    {
        var logic = _endgameLogic(panel);
        StartCoroutine(logic);
        SoundtrackManager.PlayMenu();
    }

    public static void WinGame()
    {
        instance._StartEndgame(instance.YouWinPanel);
        SoundtrackManager.PlayMenu();
    } 
    IEnumerator _endgameLogic(GameObject panel)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(waitBeforeEndingTime);
        SceneManager.LoadSceneAsync(0);
    }
}
