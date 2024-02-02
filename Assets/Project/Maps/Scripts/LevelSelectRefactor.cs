using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
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
        interactable.activated.AddListener(OnSelectLevel);
        _SetColor();

    }
    MeshRenderer mr;
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
    public void OnSelectLevel(ActivateEventArgs arg0)
    {
        if (levelSelectData == null)
        {
            Debug.LogError($"No level data assigned to {gameObject.name}", gameObject);
            return;
        }
        FadeScreen.Fade_Out(1f);
        StartCoroutine(_SelectAfter(1f));
        //SceneManager.LoadSceneAsync(levelSelectData.sceneName);
        return;
        if (levelSelectData.sceneName != "")
        {
            SceneTransitionManager.singleton.LoadScene(levelSelectData.sceneName);
            return;
        }
        
        //SceneTransitionManager.singleton.GoToScene(levelSelectData.sceneToLoad);
    }
    IEnumerator _SelectAfter(float t)
    {
        yield return new WaitForSeconds(t);
        SceneManager.LoadSceneAsync(levelSelectData.sceneName);
    }

}
