using UnityEngine;
using FishNet;
using static Player;

public class SpawnCollisionObjectHandler : MonoBehaviour
{
    int barrierDamage = 100;

    private void OnTriggerEnter(Collider other) 
    {
        if (InstanceFinder.IsClientStarted)         //If client {show visual effects, play impact audio}
        {
            //Show VFX //Play Audio.
        }

        if (InstanceFinder.IsServerStarted)    // IF SERVER, but remove if ObserverRPC
        {
            // Debug.Log("SpawnCollisionObjectHandler: OnTriggerEnter");

            ControlledShip controlledShip = other.gameObject.GetComponentInParent<ControlledShip>();

            // ControlledSpectator controlledSpectator = other.gameObject.GetComponentInParent<ControlledSpectator>();
            // if (controlledSpectator != null) { controlledSpectator.transform.position = Vector3.zero; } 

            if (controlledShip == null) {return;}

            if (transform.parent.name == "Spawn_Red" && controlledShip.player.Value.playerTeam.Value == Team.Blue) 
            {
                // Debug.Log(controlledShip.player.Value.playerTeam.Value);
                controlledShip.ReceiveDamage(barrierDamage, null); 
            }

            if (transform.parent.name == "Spawn_Blue" && controlledShip.player.Value.playerTeam.Value == Team.Red) 
            {
                controlledShip.ReceiveDamage(barrierDamage, null);
            }
        }
    }
}

