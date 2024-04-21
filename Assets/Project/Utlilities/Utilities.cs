using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static partial class Utilities
{
    public static float duration(this PlayableAsset asset)
    {
        return (float)asset.duration;
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
    /// <summary>
    /// Returns the directional vector from origin to target
    /// </summary>
    /// <param name="origin">The source</param>
    /// <param name="target"></param>
    /// <param name="normalized">Whether to return the vector normalized or not</param>
    /// <returns></returns>
    public static Vector3 DirectionTo(this Vector3 origin,  Vector3 target, bool normalized = true)
    {
        Vector3 dir = target - origin;
        if (normalized)
            dir = dir.normalized;
        return dir;
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
    public static string PreciseString(this Vector2 vector, int round = -1)
    {
        string precise;
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
        return files.ToList();
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
    public static float NormalizeAngle(this float degrees)
    {
        var normalized = degrees % 360;
        if (normalized < 0)
            normalized += 360;
        return normalized;
    }
    
    public static Vector3 RandomPointInside(this Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}