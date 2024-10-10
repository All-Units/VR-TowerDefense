using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderAsync : MonoBehaviour
{
    static SceneLoaderAsync instance;
    /// <summary>
    /// Only one scene loader can exist at a time
    /// </summary>
    /// <param name="sceneName"></param>
    public static void LoadScene(string sceneName)
    {
        //Do nothing if there is already a scene loader in existence
        if (instance != null)
        {
            Debug.LogError($"Error loading: {sceneName}, a scene loader already exists! Loading: {_currentScene}");
            return;
        }
        _currentScene = sceneName;
        var go = new GameObject("SceneLoader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneLoaderAsync>();
        instance.StartCoroutine(_LoadScene());
        
    }
    static string _currentScene;

    static IEnumerator _LoadScene()
    {
        AsyncOperation loader = SceneManager.LoadSceneAsync(_currentScene);
        
        while (loader.isDone == false)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        GameObject toDestroy = instance.gameObject;
        instance = null;
        Destroy(toDestroy);
    }
}
