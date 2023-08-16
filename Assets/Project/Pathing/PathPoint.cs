using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PathPoint : MonoBehaviour
{
    [SerializeField] public PathPoint nextPoint;
    [SerializeField] public PathPoint prevPoint;
    public Vector3 position;
    public float DistanceToGoal = float.PositiveInfinity;
    [HideInInspector] public PathPoint goal;
    private void Awake()
    {
        position = transform.position;
        goal = nextPoint;
        if (goal == null)
            goal = this;
        while (goal && goal.nextPoint != null)
            goal = goal.nextPoint;
        DistanceToGoal = Vector3.Distance(position, goal.transform.position);
    }

    public PathPoint GetNext() => nextPoint;
    public Vector3 GetPoint() => transform.position;

    protected virtual void OnDrawGizmos()
    {
        var position = transform.position;

        if (nextPoint == null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(position, .5f);
        }
        

        var next = nextPoint;
        var start = next;
        while (next)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(position, next.transform.position);

            position = next.transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(position, .5f);

            next = next.nextPoint;
            if (next == start)
                break;
        }
    }

    [MenuItem("PathPoint/CreateNewPathPointFromSelected ^g"), MenuItem("GameObject/Pathing/New Connected Path Point")]
    public static void CreateNewPathPointFromSelected(MenuCommand menuCommand)
    {
        Debug.Log("Creating Path Points");
        var newPoints = new List<GameObject>();
        foreach (var selected in Selection.gameObjects)
        {
            if(selected.TryGetComponent(out PathPoint pathPoint) == false) continue;

            var newPoint = new GameObject(selected.name.IterateSuffix());
            var pp = newPoint.AddComponent<PathPoint>();
            pathPoint.nextPoint = pp;
            pp.prevPoint = pathPoint;
            pp.nextPoint = null;
            newPoint.transform.SetParent(selected.transform.parent);
            newPoint.transform.position = selected.transform.position;
            newPoints.Add(newPoint);
        }

        Selection.objects = newPoints.ToArray();
    }   
    

    
    [MenuItem("GameObject/Pathing/New Path Point")]
    public static void CreateNewPathPoint()
    {
        var newPoint = new GameObject("New Path Point");
        newPoint.AddComponent<PathPoint>();
    }
}