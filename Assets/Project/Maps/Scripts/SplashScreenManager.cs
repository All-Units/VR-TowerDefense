using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
public class SplashScreenManager : MonoBehaviour
{
    public TeleportationProvider teleporter;
    public GameLevel_SO MainMenu;
    public GameLevel_SO Tutorial;
    public float DisplayTime = 4f;
    

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(_LoadMainMenu());
    }
    
    
    IEnumerator _LoadMainMenu()
    {
        PlayableDirector pd = GetComponentInChildren<PlayableDirector>();
        if (pd != null)
        {
            DisplayTime = (float)pd.duration;
        }
        yield return new WaitForSeconds(DisplayTime);
        var loader = SceneManager.LoadSceneAsync(_GetScene());
        float t = Time.time;
        while (loader.isDone == false)
        {
            yield return null;
        }
    }

    string _GetScene()
    {
        if (PlayerPrefs.GetInt("_has_completed_tutorial", 0) == 0)
        {
            return Tutorial.levelTitle;
        }
        return MainMenu.levelTitle;
    }

}
