using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerAimer : MonoBehaviour
{
    [SerializeField] Transform gunParent;
    ProjectileTower tower;
    // Start is called before the first frame update
    void Start()
    {
        tower = GetComponentInParent<ProjectileTower>();
    }

    // Update is called once per frame
    void Update()
    {
        _AimTower();
    }
    void _AimTower()
    {
        if (tower == null || tower.GetCurrentTarget == null) return;
        if (gunParent == null) return;

        Vector3 target = tower.GetCurrentTarget.position;

        Vector3 flat = target; flat.y = transform.position.y;
        transform.LookAt(flat);

        gunParent.LookAt(target);
    }
}
