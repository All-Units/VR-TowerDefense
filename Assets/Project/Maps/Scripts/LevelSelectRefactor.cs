using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class LevelSelectRefactor : MonoBehaviour
{
    [SerializeField] private LevelSelectData levelSelectData;
    [SerializeField] XRSimpleInteractable interactable;
    [SerializeField] Transform displayParent;
    [SerializeField] Transform textCanvas;

    public float SelectedSize = 2f;
    Vector3 _selectedSize => Vector3.one * SelectedSize;
    public float IdleSize = 1f;
    Vector3 _idleSize => Vector3.one * IdleSize;
    Vector3 _textIdleSize = Vector3.zero;
    Vector3 _textSelectedSize = Vector3.one;

    public float ChangeSizeTime = 0.5f;

    [SerializeField] XRSimpleInteractable LoadLevelBubble;
    [SerializeField] XRSimpleInteractable NewLevelBubble;

    private void OnDrawGizmos()
    {
        if (levelSelectData == null) return;
#if UNITY_EDITOR
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>();
        if (gameObject.name != levelSelectData.title ||
            text.text != levelSelectData.title)
            EditorUtility.SetDirty(gameObject);
        
        gameObject.name = levelSelectData.title;
        
        text.text = levelSelectData.title;
#endif
    }
    TextMeshProUGUI text;
    private void Awake()
    {
        _Deselect();
        text = GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = levelSelectData.title;
        if (interactable == null) interactable = GetComponent<XRSimpleInteractable>();

        //Add all selectors
        interactable.selectEntered.AddListener(_Select);
        interactable.hoverEntered.AddListener(_Select);


        //Add deselectors
        interactable.selectExited.AddListener(_Deselect);
        interactable.hoverExited.AddListener(_Deselect);


        //On trigger pull
        interactable.activated.AddListener(OnActivateLevel);
        _SetColor();

        LoadLevelBubble.gameObject.SetActive(false);
        NewLevelBubble.gameObject.SetActive(false);
        LoadLevelBubble.activated.AddListener(OnActivateLoad);

        NewLevelBubble.activated.AddListener(_StartLoadLevel);

    }
    MeshRenderer mr;

    void OnActivateLoad(ActivateEventArgs args)
    {
        print($"Clicked load!. Save exists? {_SaveFileExists}");
        if (_SaveFileExists == false) return;

        SerializationManager.LoadLevelNext(levelSelectData);
        _StartLoadLevel(args);
    }
    public void _SetColor()
    {
        if (levelSelectData.OverrideTowerColor == null) return;
        if (mr == null) mr = GetComponentInChildren<MeshRenderer>();
        var colors = mr.materials;

        colors[1] = levelSelectData.OverrideTowerColor;
        
    }

    

    private void _Select(HoverEnterEventArgs arg0)
    {
        _Select();
    }

    bool isSelected = false;
    private void _Select(SelectEnterEventArgs args)
    {
        isSelected = true;
        _Select();
    }

    IEnumerator _ChangeSizeRoutine(bool selected = true)
    {
        //The desired target size, if we are selected or not
        Vector3 target = selected ? _selectedSize : _idleSize;
        Vector3 textTarget = selected ? _textSelectedSize : _textIdleSize;
        float t = 0;
        while (t <= ChangeSizeTime)
        {
            Vector3 scale = Vector3.Slerp(displayParent.localScale, target, t);
            Vector3 textScale = Vector3.Slerp(textCanvas.localScale, textTarget, t);
            t += Time.deltaTime;
            displayParent.localScale = scale;
            textCanvas.localScale = textScale;
            yield return null;
        }
        _currentSizeChanger = null;
    }
    IEnumerator _currentSizeChanger = null;

    void _StartChangerRoutine(bool selected = true)
    {
        if (gameObject.activeInHierarchy == false) return;
        if (_currentSizeChanger != null)
            StopCoroutine(_currentSizeChanger);
        _currentSizeChanger = _ChangeSizeRoutine(selected);
        StartCoroutine(_currentSizeChanger);
    }
    void _Select()
    {
        _StartChangerRoutine(true);
    }

    

    void _Deselect()
    {
        _StartChangerRoutine(false);
    }
    private void _Deselect(HoverExitEventArgs arg0)
    {
        if (isSelected == false)
            _Deselect();
    }

    private void _Deselect(SelectExitEventArgs arg0)
    {
        isSelected = false;
        _Deselect();
    }

    bool _SaveFileExists => File.Exists(_SaveFilePath());
    

    string _SaveFilePath()
    {
        if (levelSelectData == null) return "";

        string path = $"{Application.persistentDataPath}/{levelSelectData.sceneName}.dat";
        return path;
    }
    bool _AreBubblesActive => LoadLevelBubble.gameObject.activeInHierarchy || NewLevelBubble.gameObject.activeInHierarchy;
    void _ActivateBubbles(bool active)
    {
        if (_bubblesDirector == null)
            _bubblesDirector = LoadLevelBubble.GetComponentInParent<PlayableDirector>();
        if (_bubblesDirector != null)
            _bubblesDirector.Play();
        LoadLevelBubble.gameObject.SetActive(active);
        NewLevelBubble.gameObject.SetActive(active);    
    }
    PlayableDirector _bubblesDirector;

    static LevelSelectRefactor _currentRefactor = null;
    public void OnActivateLevel(ActivateEventArgs arg0)
    {
        //If there is no save, load and return
        if (_SaveFileExists == false)
        {
            _StartLoadLevel();
            return;
        }

        if (_currentRefactor != null && _currentRefactor != this)
        {
            _currentRefactor._ActivateBubbles(false);
        }
        _currentRefactor = this;
        //Invert bubbles active
        _ActivateBubbles(_AreBubblesActive == false);
        return;
    }
    void _StartLoadLevel(ActivateEventArgs args = null)
    {
        if (levelSelectData == null)
        {
            Debug.LogError($"No level data assigned to {gameObject.name}", gameObject);
            return;
        }
        FadeScreen.Fade_Out(1f);
        StartCoroutine(_SelectAfter(1f));
    }
    IEnumerator _SelectAfter(float t)
    {
        yield return new WaitForSeconds(t);
        SceneManager.LoadSceneAsync(levelSelectData.sceneName);
    }

}
