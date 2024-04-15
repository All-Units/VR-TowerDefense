using UnityEngine;

public class TowerAimer : MonoBehaviour
{
    [SerializeField] Transform gunParent;
    public bool CalculateArc = true;
    ProjectileTower tower;
    // Start is called before the first frame update
    void Start()
    {
        tower = GetComponentInParent<ProjectileTower>();
    }
    Vector3 pos;
    public Vector3 CurrentTarget;
    // Update is called once per frame
    void Update()
    {
        pos = transform.position;
        _AimTower();
    }
    Vector3 _lastTarget;
    bool BASIC = false;
    void _AimTower()
    {
        if (tower == null || tower.GetCurrentTarget == null) return;
        if (gunParent == null) return;

        Vector3 target = tower.GetCurrentTarget.position + Vector3.up;

        //If they get too close, aim up a bit
        if (target.FlatDistance(pos) < 7f)
            target.y += 0f;
        if (BASIC)
        {
            transform.LookAt(target);
            return;
        }


        _lastTarget = target;
        Vector3 flat = target; flat.y = pos.y;
        transform.LookAt(flat);
        Vector3 ballisticTarget = solve_ballistic_arc(pos, tower.ProjectileSpeed, target);
        CurrentTarget = ballisticTarget;
        ballisticTarget += pos;
        if (CalculateArc == false)
            ballisticTarget = target;
        gunParent.LookAt(ballisticTarget);
    }
    private void OnDrawGizmos()
    {
        //return;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, _lastTarget);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + CurrentTarget);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(_lastTarget, 0.5f);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="proj_pos"></param>
    /// <param name="proj_speed"></param>
    /// <param name="target"></param>
    /// <param name="gravity"></param>
    /// <returns>If returns Vector3.zero, there are no solutions</returns>
    public static Vector3 solve_ballistic_arc(Vector3 proj_pos, float proj_speed, Vector3 target, float gravity = 9.8f)
    {

        // Handling these cases is up to your project's coding standards
        Debug.Assert(proj_pos != target && proj_speed > 0 && gravity > 0, "fts.solve_ballistic_arc called with invalid data");

        // C# requires out variables be set
        var s0 = Vector3.zero;
        var s1 = Vector3.zero;

        // Derivation
        //   (1) x = v*t*cos O
        //   (2) y = v*t*sin O - .5*g*t^2
        // 
        //   (3) t = x/(cos O*v)                                        [solve t from (1)]
        //   (4) y = v*x*sin O/(cos O * v) - .5*g*x^2/(cos^2 O*v^2)     [plug t into y=...]
        //   (5) y = x*tan O - g*x^2/(2*v^2*cos^2 O)                    [reduce; cos/sin = tan]
        //   (6) y = x*tan O - (g*x^2/(2*v^2))*(1+tan^2 O)              [reduce; 1+tan O = 1/cos^2 O]
        //   (7) 0 = ((-g*x^2)/(2*v^2))*tan^2 O + x*tan O - (g*x^2)/(2*v^2) - y    [re-arrange]
        //   Quadratic! a*p^2 + b*p + c where p = tan O
        //
        //   (8) let gxv = -g*x*x/(2*v*v)
        //   (9) p = (-x +- sqrt(x*x - 4gxv*(gxv - y)))/2*gxv           [quadratic formula]
        //   (10) p = (v^2 +- sqrt(v^4 - g(g*x^2 + 2*y*v^2)))/gx        [multiply top/bottom by -2*v*v/x; move 4*v^4/x^2 into root]
        //   (11) O = atan(p)

        Vector3 diff = target - proj_pos;
        Vector3 diffXZ = new Vector3(diff.x, 0f, diff.z);
        float groundDist = diffXZ.magnitude;

        float speed2 = proj_speed * proj_speed;
        float speed4 = proj_speed * proj_speed * proj_speed * proj_speed;
        float y = diff.y;
        float x = groundDist;
        float gx = gravity * x;

        float root = speed4 - gravity * (gravity * x * x + 2 * y * speed2);

        // No solution
        if (root < 0)
            return Vector3.zero;

        root = Mathf.Sqrt(root);

        float lowAng = Mathf.Atan2(speed2 - root, gx);
        float highAng = Mathf.Atan2(speed2 + root, gx);
        int numSolutions = lowAng != highAng ? 2 : 1;

        Vector3 groundDir = diffXZ.normalized;
        s0 = groundDir * Mathf.Cos(lowAng) * proj_speed + Vector3.up * Mathf.Sin(lowAng) * proj_speed;
        if (numSolutions > 1)
            s1 = groundDir * Mathf.Cos(highAng) * proj_speed + Vector3.up * Mathf.Sin(highAng) * proj_speed;
        Vector3 solution = s0;
        if (s1 != Vector3.zero)
            solution = s1;
        return s0;
    }
}
