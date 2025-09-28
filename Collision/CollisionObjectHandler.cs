using UnityEngine;
using FishNet;
using static Player;

public class CollisionObjectHandler : MonoBehaviour
{
    private float proximityDistance = 50f; // Define the proximity distance for when mesh renderer is enabled
    
    private float xDistance;
    private float zDistance;

    private MeshRenderer meshRenderer;
    private Transform playerObject;

    CollisionObjectHandler[] collisionObjects;

    int barrierDamage = 1;


    private void Awake()
    {
        FirstObjectNotifier.OnFirstObjectSpawned += FirstObjectNotifier_OnFirstObjectSpawned; // Subscribe to FirstObjectNotifier
        meshRenderer = GetComponent<MeshRenderer>(); // Get mesh renderer 
        collisionObjects = FindObjectsOfType<CollisionObjectHandler>();

        //meshRenderer.enabled = false;
        //meshRenderer.enabled = true;
    }

    private void FirstObjectNotifier_OnFirstObjectSpawned(Transform obj)
    {
        playerObject = obj; 
    }

    private void OnCollisionEnter (Collision collision)
    {
        // Debug.Log("CollisionObjectHandler: OnCollisionEnter");
        ProcessCollision(collision.collider);
    }

    private void OnTriggerEnter(Collider other) 
    {
        // Debug.Log("CollisionObjectHandler: OnTriggerEnter");
        ProcessCollision(other);
    }

    private void ProcessCollision(Collider other)
    {
        if (InstanceFinder.IsClientStarted)         //If client {show visual effects, play impact audio}
        {
            //Show VFX //Play Audio.
        }
        
        if (InstanceFinder.IsServerStarted)    // IF SERVER, but remove if ObserverRPC
        {
            ControlledShip controlledShip = other.gameObject.GetComponentInParent<ControlledShip>();
            
            if (controlledShip == null) {return;}

            // Debug.Log("CollisionObjectHandler: controlledShip.player.Value.playerTeam.Value: " + controlledShip.player.Value.playerTeam.Value);

            if (controlledShip.player.Value.playerTeam.Value == Team.Red) 
            {
                // Debug.Log("CollisionObjectHandler: ReceiveDamage");
                controlledShip.ReceiveDamage(barrierDamage, null); 
            }

            if (controlledShip.player.Value.playerTeam.Value == Team.Blue) 
            {
                // Debug.Log("CollisionObjectHandler: ReceiveDamage");
                controlledShip.ReceiveDamage(barrierDamage, null);
            }
        }
    }
}


    // private void Update()
    // {

    //     if (playerObject == null) return;

    //     bool isWithinProximity = false; 
    //     // Debug.Log("isWithinProximity = " + isWithinProximity);
    //     // Debug.Log("MeshRendererEnabled = " + meshRenderer.enabled);            // TODO   False True at the same time!

    //     foreach (CollisionObjectHandler obj in collisionObjects)
    //     {
    //         xDistance = Mathf.Abs(transform.position.x - playerObject.position.x);   // transform.position = position of CollisionObjectHander.cs
    //         zDistance = Mathf.Abs(transform.position.z - playerObject.position.z);

    //         if (xDistance <= proximityDistance || zDistance <= proximityDistance)
    //         {
    //             isWithinProximity = true;
    //             break; // Break the loop if at least one object is within proximity distance
    //         }
    //     }

    //     meshRenderer.enabled = isWithinProximity;

    //     // if (isWithinProximity)
    //     // {
    //     //     meshRenderer.enabled = true;
    //     // }
    //     // if (!isWithinProximity)
    //     // {
    //     //     meshRenderer.enabled = false;
    //     // }
    // }
