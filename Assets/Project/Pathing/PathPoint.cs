

using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PathPoint : MonoBehaviour
{
    [SerializeField] public PathPoint nextPoint;
    [SerializeField] public PathPoint prevPoint;

    public Vector3 position;
    public float DistanceToGoal = float.PositiveInfinity;
    public float DistanceToGoalFrom(Vector3 point)
    {
        float d = position.FlatDistance(point);// Vector3.Distance(position, point);
        return d + DistanceToGoal;
    }

    [HideInInspector] public PathPoint goal;
    public int Variance = 2;

    private void Awake()
    {
        position = transform.position;
        goal = nextPoint;
        if (goal == null)
            goal = this;
        while (goal && goal.nextPoint != null)
            goal = goal.nextPoint;
        DistanceToGoal = _CalculateDistance();
    }

    public PathPoint Next => nextPoint;
    public PathPoint Prev => prevPoint;
    public Vector3 GetPoint(float tolerance)
    {
        float variance = Variance - tolerance;

        var point = position + new Vector3(Random.Range(-variance, variance), 0, Random.Range(-variance, variance));
        lastPoint = point;
        //print($"I am {gameObject.name}, position {position}, I returned the point {point}. Variance was {variance}. Distance was {Vector3.Distance(position, lastPoint)}");
        return point;
    }
    public float _CalculateDistance()
    {
        var d = 0f;
        if (nextPoint == null)
            return d;
        d += Vector3.Distance(position, nextPoint.position);
        d += nextPoint._CalculateDistance();

        return d;
    }

Vector3 lastPoint = Vector3.zero;

#if UNITY_EDITOR
    

    protected virtual void OnDrawGizmos()
    {
        gameObject.name = $"Path point {transform.GetSiblingIndex() + 1}";
        this.position = transform.position;
        var pos = position;
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
            var nextPos = next.transform.position;
            Vector3 dir = position - nextPos;
            dir.y = 0f;
            transform.LookAt(nextPos);

            foreach (var i in new[] { -1, 1 })
            {
                Vector3 a = position + transform.right * Variance * i;
                Vector3 b = nextPos + next.transform.right * next.Variance * i;
                Gizmos.DrawLine(a, b);
            }

            //Gizmos.DrawLine(position, position + transform.right * Variance);
            //Gizmos.DrawLine(position, position + transform.right * Variance * -1);
            if (position == pos)
                Gizmos.DrawWireSphere(position, Variance);

            position = next.transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(position, .5f);


            //next = next.nextPoint;
            if (next == start)
                break;
            break;
        }

        if (nextPoint == null)
        {
            transform.SetAsLastSibling();
            int i = transform.childCount - 2;
            var p = prevPoint;
            DistanceToGoal = 0f;
            var distance = DistanceToGoal;
            Vector3 last = position;
            while (p != null)
            {
                p.transform.SetAsFirstSibling();
                distance += Vector3.Distance(p.transform.position, last);
                last = p.transform.position;
                p.DistanceToGoal = distance;

                p = p.prevPoint;


            }

        }
        position = pos;
        DistanceToGoal = _CalculateDistance();
    }

    [MenuItem("PathPoint/CreateNewPathPointFromSelected ^g"), MenuItem("GameObject/Pathing/New Connected Path Point")]
    public static void CreateNewPathPointFromSelected(MenuCommand menuCommand)
    {
        Debug.Log("Creating Path Points");
        var newPoints = new List<GameObject>();
        foreach (var selected in Selection.gameObjects)
        {
            if (selected.TryGetComponent(out PathPoint pathPoint) == false) continue;

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
    [MenuItem("PathPoint/CreateNewPathPointAfterSelected ^h"), MenuItem("GameObject/Pathing/New Connected Path Point")]
    public static void CreateNewPointAfterSelected(MenuCommand menuCommand)
    {
        Debug.Log("Creating Point AFTER");
        var newPoints = new List<GameObject>();
        foreach (var selected in Selection.gameObjects)
        {
            if (selected.TryGetComponent(out PathPoint pathPoint) == false) continue;

            var newPoint = new GameObject(selected.name.IterateSuffix());
            var pp = newPoint.AddComponent<PathPoint>();

            var oldNext = pathPoint.nextPoint;
            pathPoint.nextPoint = pp;

            pp.nextPoint = oldNext;
            oldNext.prevPoint = pp;
            pp.prevPoint = pathPoint;


            //pathPoint.nextPoint = pp;
            //pp.prevPoint = pathPoint;
            //pp.nextPoint = null;
            newPoint.transform.SetParent(selected.transform.parent);
            newPoint.transform.position = selected.transform.position;
            newPoint.transform.SetSiblingIndex(selected.transform.GetSiblingIndex() + 1);
            newPoints.Add(newPoint);
            break;
        }

        Selection.objects = newPoints.ToArray();
    }



    [MenuItem("GameObject/Pathing/New Path Point")]
    public static void CreateNewPathPoint()
    {
        var newPoint = new GameObject("New Path Point");
        newPoint.AddComponent<PathPoint>();
    }
    [MenuItem("CONTEXT/PathPoint/DeletePoint")]
    public static void DeletePoint()
    {
        var sel = (GameObject)Selection.objects.FirstOrDefault();
        var path = sel.GetComponent<PathPoint>();
        if (sel == null || path == null) return;

        var prev = path.prevPoint;
        var next = path.nextPoint;
        if (next != null)
            next.prevPoint = prev;
        if (prev != null)
            prev.nextPoint = next;
        DestroyImmediate(sel);
    }
#endif

}
