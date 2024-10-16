using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

#endif
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class Utilities
{
    /// <summary>
    /// Starts a coroutine that destroys GO after a delay. Pauses delay if game is paused
    /// </summary>
    /// <param name="go"></param>
    /// <param name="t"></param>
    public static void DestroyAfter(this GameObject go, float t)
    {
        MonoBehaviour monoBehaviour = go.GetComponent<MonoBehaviour>();
        if (monoBehaviour == null) return;
        monoBehaviour.StartCoroutine(go._DestroyAfter(t));
    }
    public static IEnumerator _DestroyAfter(this GameObject go, float t)
    {
        float current = 0f;
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
    public static Vector2 RandomPointOnUnitCircle()
    {
        var seed = Random.Range(0, 1f);
        var theta = seed * 2 * Mathf.PI;
        return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
    }

    public static Vector2 IncrementsOfUnitCircle(int increment, int degree = 90)
    {
        var theta = increment*degree*Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta));
    }
    /// <summary>
    /// Returns the distance between two vectors, ignoring their Y values
    /// </summary>
    /// <param name="a">The first vector</param>
    /// <param name="b">The second vector</param>
    /// <returns></returns>
    public static float FlatDistance(this Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    public static bool RandomNavSphere(Vector3 origin, float dist, out Vector3 result, int layerMask = NavMesh.AllAreas) 
    {
        var randDirection = Random.insideUnitSphere * dist;
 
        randDirection += origin;

        if (NavMesh.SamplePosition(randDirection, out var navHit, dist, layerMask))
        {
            result = navHit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
    /// <summary>
    /// Gets all children in a transform, then gets all THEIR children, recursively
    /// </summary>
    /// <param name="t">The transform to get the children of</param>
    /// <param name="returnSelf"></param>
    /// <returns></returns>
    public static HashSet<Transform> GetAllDescendants(this Transform t, bool returnSelf = true)
    {
        HashSet<Transform> result = new HashSet<Transform>();
        if (returnSelf)
            result.Add(t);
        for (int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i);
            result.Add(child);
            var children = child.GetAllDescendants();
            result.UnionWith(children);
        }
        return result;
    }
    
    /// <summary>
    /// Prints a formatted Vector 2 string, to an arbitrary precision
    /// </summary>
    /// <param name="vector">The vector to print</param>
    /// <param name="round">How many digits to round to. If left at default -1, doesn't round</param>
    /// <returns></returns>
    public static string preciseString(this Vector2 vector, int round = -1)
    {
        string precise = "";
        if (round == -1)
        {
            precise = $"({vector.x}, {vector.y})";
        }

        else
        {
            double x = Math.Round(vector.x, round);
            double y = Math.Round(vector.y, round);
            
            precise = $"({x}, {y})";
        }


        return precise;
    }

    public static string preciseVector3IntString(this Vector3 vector, int round = -1)
    {
        return ((Vector3)Vector3Int.RoundToInt(vector)).preciseVector3String(round);
    }
    /// <summary>
    /// Prints a formatted Vector 3 string, to an arbitrary precision
    /// </summary>
    /// <param name="vector">The vector to print</param>
    /// <param name="round">How many digits to round to. If left at default -1, doesn't round</param>
    /// <returns></returns>
    public static string preciseVector3String(this Vector3 vector, int round = -1)
    {
        string precise = "";
        if (round == -1)
        {
            precise = $"({vector.x}, {vector.y}, {vector.z})";
        }

        else
        {
            double x = Math.Round(vector.x, round);
            double y = Math.Round(vector.y, round);
            double z = Math.Round(vector.z, round);
            
            precise = $"({x}, {y}, {z})";
        }

        return precise;
    }
    /// <summary>
    /// Gets the full path for any transform, through all of its parents
    /// </summary>
    /// <param name="t">The transform to get the full path of</param>
    /// <returns>Returns a string representation of the path to the tranform</returns>
    public static string FullPath(this Transform t)
    {
        string s = "";
        Transform current = t;
        while (current != null)
        {
            s = $"{current.name}/{s}";
            current = current.parent;
        }
        return s;
    }
    /// <summary>
    /// An overload of <see cref="FullPath(UnityEngine.Transform)"/> for GameObjects
    /// </summary>
    /// <param name="go">The GameObject</param>
    /// <returns>Returns go.transform.FullPath</returns>
    public static string FullPath(this GameObject go)
    {
        return go.transform.FullPath();
    }

#if UNITY_EDITOR
    [MenuItem("CONTEXT/Component/Move To New Child GameObject")]
    public static void MoveToNewChildGameObject(MenuCommand command)
    {
        if(!(command.context is Component component)) return;

        var child = new GameObject("New child");
        child.transform.SetParent(component.transform);

        var type = component.GetType();
        var copy = child.AddComponent(type);
        
        // Copied fields can be restricted with BindingFlags
        var fields = type.GetFields(); 
        foreach (var field in fields)
        {
            field.SetValue(copy, field.GetValue(component));
        }
        
        Object.DestroyImmediate(component);
    }
    /// <summary>
    /// [NYE] Gets all of the items in the folder contained in root. Starts at Application.path,
    /// which includes /Assets/
    /// </summary>
    /// <param name="root">The root folder to search.</param>
    /// <param name="types">Comma-seperated list of filetypes to include. Ex '.fbx,.mp4'</param>
    /// <param name="recursive">If we recurse to search</param>
    /// <param name="includeFolders">Whether to include folder paths as well</param>
    /// <returns></returns>
    public static List<string> GetContentsOfFolder(string root,
        string types = "",
        bool recursive = true,
        bool includeFolders = true)
    {
        
        string folder = Application.dataPath + root;
        string[] files = Directory.GetFiles(folder);
        List<string> contents = new List<string>();
        foreach (string s in files)
        {
            
        }
        return contents;
    }
#endif

    public static T GetRandom<T>(this List<T> list)
    {
        //Random.range is exclusive on the upper bound
        return list[Random.Range(0, list.Count)];
    }
    public static int GetRandomIndex<T>(this List<T> list)
    {
        //Random.range is exclusive on the upper bound
        return Random.Range(0, list.Count);
    }
    
    public static Vector3 RandomPointInBounds(this Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
    
    public static string IterateSuffix(this string str)
    {
        var sub = str.Split('_');
        var last = sub[^1];
        if (int.TryParse(last, out var n))
        {
            sub[^1] = (n + 1).ToString();
            str = string.Join("_",sub);
        }

        return str;
    }

        public static void SetLayerRecursive(this GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
        {
            t.gameObject.SetLayerRecursive(layer);
        }
    }
    /// <summary>
    /// Destroys all children of a transform
    /// </summary>
    /// <param name="p">The transform to destroy the children of</param>

    public static void DestroyChildren(this Transform p)
    {
        var oldChildren = (from Transform t in p.transform select t.gameObject).ToList();

        while (oldChildren.Count > 0)
        {
            var child = oldChildren[0];
            oldChildren.RemoveAt(0);
            child.transform.parent = null;
            Object.Destroy(child);
        }
    }    
    
    public static void DestroyChildrenImmediate(this Transform p)
    {
        var oldChildren = (from Transform t in p.transform select t.gameObject).ToList();

        while (oldChildren.Count > 0)
        {
            var child = oldChildren[0];
            oldChildren.RemoveAt(0);
            child.transform.parent = null;
            Object.DestroyImmediate(child);
        }
    }
    
    public static float Remap (this float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static bool InRange(this Array array, int idx)
    {
        return (idx >= 0 && idx < array.Length);
    }

    public static bool InRange(this IList list, int idx)
    {
        return (idx >= 0 && idx < list.Count);
    }

    public static int Factorial(this int v)
    {
        var n = v;
        var ret = 1;
        while (n >= 1)
        {
            ret *= n;
            n--;
        }

        return ret;
    }
    
    public static Vector3 RandomPointInside(this Bounds bounds) {
        return new Vector3(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
            UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
        );
    }
    
    public static InputAction GetInputAction(InputActionReference actionReference)
    {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
        return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
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
