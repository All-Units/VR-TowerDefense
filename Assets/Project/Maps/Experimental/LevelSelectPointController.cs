using System;
using TMPro;
using UnityEngine;

public class LevelSelectPointController : MonoBehaviour
{
    [SerializeField] private LevelSelectData levelSelectData;
    [SerializeField] private TMP_Text titleText;

    private void Awake()
    {
        if(levelSelectData != null)
            titleText.text = levelSelectData.title;
    }

    private void OnValidate()
    {
        if(levelSelectData != null && titleText != null)
            titleText.text = levelSelectData.title;
    }

    public void OnSelectLevel()
    {
        if (levelSelectData == null)
        {
            Debug.LogError($"No level data assigned to {gameObject.name}", gameObject);
            return;
        }
        
        SceneTransitionManager.singleton.GoToScene(levelSelectData.sceneToLoad);
    }
}