using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeepPlayerInTower : MonoBehaviour
{
    public TeleportationProvider teleportationProvider;
    public float LeashLength = 20f;
    public float currentDistance;
    public Transform player;
    Vector3 startPos;
    Quaternion startRot;
    // Start is called before the first frame update
    void Start()
    {
        teleportationProvider = GetComponentInChildren<TeleportationProvider>();
        startPos = player.transform.position;
        startRot = player.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        currentDistance = Vector3.Distance(startPos, player.transform.position);
        if (Vector3.Distance(player.transform.position, startPos) >= LeashLength)
        {
            TeleportPlayerToTower();
        }

    }
    private void TeleportPlayerToTower()
    {
        TeleportRequest request = new TeleportRequest()
        {
            requestTime = Time.time,
            matchOrientation = MatchOrientation.TargetUpAndForward,

            destinationPosition = startPos,
            destinationRotation = startRot
        };

        teleportationProvider.QueueTeleportRequest(request);
    }
}
