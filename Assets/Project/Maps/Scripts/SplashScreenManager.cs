using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
public class SplashScreenManager : MonoBehaviour
{
    public TeleportationProvider teleporter;
    public GameLevel_SO MainMenu;
    public GameLevel_SO Tutorial;
    public float DisplayTime = 4f;
    [SerializeField] float _timeToSkipRound = 0.5f;
    [SerializeField] InputActionReference skipButton;
    InputAction skipRoundAction => Utilities.GetInputAction(skipButton);

    IEnumerator _loader = null;
    // Start is called before the first frame update
    void Start()
    {
        _loader = _LoadMainMenu();
        StartCoroutine(_loader);
        skipRoundAction.started += SkipRoundAction_started;
        skipRoundAction.canceled += SkipRoundAction_canceled;

    }
    private void OnDestroy()
    {
        skipRoundAction.started -= SkipRoundAction_started;
        skipRoundAction.canceled -= SkipRoundAction_canceled;
    }


    IEnumerator _LoadMainMenu()
    {
        PlayableDirector pd = GetComponentInChildren<PlayableDirector>();
        if (pd != null)
        {
            DisplayTime = (float)pd.duration;
        }
        yield return new WaitForSeconds(DisplayTime);

        LoadScene();
        //var loader = SceneManager.LoadSceneAsync(_GetScene());
        //float t = Time.time;
        //while (loader.isDone == false)
        {
            //yield return null;
        }
    }
    void LoadScene()
    {
        SceneLoaderAsync.LoadScene(_GetScene());
    }

    string _GetScene()
    {
        if (PlayerPrefs.GetInt("_has_completed_tutorial", 0) == 0)
        {
            return Tutorial.levelTitle;
        }
        return MainMenu.levelTitle;
    }
    GameObject skipPanelPrefab => Resources.Load<GameObject>("Prefabs/SkipRoundCanvas");
    public Transform cam;
    GameObject _currentSkipPanel = null;
    bool isSkipPressed = false;

    private void SkipRoundAction_started(InputAction.CallbackContext obj)
    {
        
        isSkipPressed = true;
        if (PlayerPrefs.GetInt("_has_completed_tutorial", 0) == 0) return;
        _currentSkipRoutine = _SkipRound();
        StartCoroutine(_currentSkipRoutine);

    }
    private void SkipRoundAction_canceled(InputAction.CallbackContext obj)
    {
        isSkipPressed = false;

    }
    IEnumerator _currentSkipRoutine = null;
    IEnumerator _SkipRound()
    {
        GameObject panel = Instantiate(skipPanelPrefab);
        _currentSkipPanel = panel;
        panel.transform.position = cam.position;

        Vector3 euler = new Vector3(0f, cam.eulerAngles.y, 0f);
        panel.transform.eulerAngles = euler;

        float t = 0f; 
        Image fill = panel.GetComponentInChildren<Image>();
        fill.fillAmount = 0f;
        while (t <= _timeToSkipRound)
        {
            if (isSkipPressed == false)
                break;
            yield return null;
            t += Time.deltaTime;
            fill.fillAmount = math.lerp(0f, 1f, (t / _timeToSkipRound));
        }
        Destroy(panel);
        if (t >= _timeToSkipRound)
        {
            yield return null;
            StopCoroutine(_loader);
            FadeScreen.Fade_Out(0.5f);
            yield return new WaitForSeconds(0.5f);
            LoadScene();
        }

        _currentSkipRoutine = null;
    }
}
