using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class Utilities
{
    /// <summary>
    /// Starts a coroutine that destroys GO after a delay. Pauses delay if game is paused
    /// </summary>
    /// <param name="go"></param>
    /// <param name="t"></param>
    public static void DestroyAfter(this GameObject go, float t)
    {
        var monoBehaviour = go.GetComponent<MonoBehaviour>();
        if (monoBehaviour == null) return;
        monoBehaviour.StartCoroutine(go._DestroyAfter(t));
    }
    
    public static IEnumerator _DestroyAfter(this GameObject go, float t)
    {
        var current = 0f;
        while (current < t)
        {
            if (XRPauseMenu.IsPaused == false)
                current += Time.deltaTime;
            else
                current += 0f;
            yield return null;
        }
        yield return null;
        Object.Destroy(go);
    }
        
#if UNITY_EDITOR
    [MenuItem("Castle Tools/Scenes/Go To DaneMainScene %#d")]
    public static void GoToDaneScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Project/Maps/Scenes/map03.unity");
        }
    }
    [MenuItem("Castle Tools/Scenes/Go To DaneSecondScene %#e")]
    public static void GoToDaneSecondScene()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Project/Maps/Scenes/map_trailer.unity");
        }
    }
    [MenuItem("Castle Tools/Scenes/Go To Main Menu %#m")]
    public static void GoToMainMenu()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene("Assets/Project/Maps/Scenes/MainMenu.unity");
        }
    }
    
#endif
}
