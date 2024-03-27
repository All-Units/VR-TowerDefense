using System;
using UnityEngine;

public class HideOnEndGame : MonoBehaviour
{
    private void Start()
    {
        GameStateManager.onGameWin += Hide;
        GameStateManager.onGameLose += Hide;
    }

    private void OnDestroy()
    {
        GameStateManager.onGameWin -= Hide;
        GameStateManager.onGameLose -= Hide;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
