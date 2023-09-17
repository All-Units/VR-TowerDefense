using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private TeleportationProvider teleportationProvider;

    public Transform penthouse;
    public Transform frontlines;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TeleportToSafety()
    {
        PlayerStateController.instance.TeleportPlayerToPenthouse();
        //TeleportPlayerToPoint(penthouse);
    }

    public void TeleportToFrontlines()
    {
        PlayerStateController.instance.TeleportPlayerToWar();
        //TeleportPlayerToPoint(frontlines);
    }
    
    private void TeleportPlayerToPoint(Transform playerControlPoint)
    {
        TeleportRequest request = new TeleportRequest()
        {
            requestTime = Time.time,
            matchOrientation = MatchOrientation.TargetUpAndForward,

            destinationPosition = playerControlPoint.position,
            destinationRotation = playerControlPoint.rotation
        };

        teleportationProvider.QueueTeleportRequest(request);

    }
}
