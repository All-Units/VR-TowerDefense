using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class Utilities
{
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
    /// Gets all of the items in the folder contained in root. Starts at Application.path,
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
}
