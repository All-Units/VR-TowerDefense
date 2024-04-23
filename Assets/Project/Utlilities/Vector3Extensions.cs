using System;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
    }

    public static Vector3 Add(this Vector3 vector, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3((x ?? 0) + vector.x, (y ?? 0) + vector.y, (z ?? 0) + vector.z); 
    }
    
    public static string PreciseVector3IntString(this Vector3 vector, int round = -1)
    {
        return ((Vector3)Vector3Int.RoundToInt(vector)).PreciseVector3String(round);
    }
    
    /// <summary>
    /// Prints a formatted Vector 3 string, to an arbitrary precision
    /// </summary>
    /// <param name="vector">The vector to print</param>
    /// <param name="round">How many digits to round to. If left at default -1, doesn't round</param>
    /// <returns></returns>
    public static string PreciseVector3String(this Vector3 vector, int round = -1)
    {
        string precise;
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
}