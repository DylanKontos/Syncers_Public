using FishNet;
using FishNet.Managing.Server;
using FishNet.Object;
using UnityEngine;

public class Powerup : NetworkBehaviour
{
    public GameObject SpawnPointOwner { get; set; }
    public PowerupEffect powerupEffect;

    BuffSpawner buffSpawner;

    private void Start()
    {
        buffSpawner = FindFirstObjectByType<BuffSpawner>(); // find the buffController/Spawner
    }

    public void SetSpawnPointOwner(GameObject spawnPointOwner)
    {
        SpawnPointOwner = spawnPointOwner;
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (InstanceFinder.IsClientStarted)       {} 

        if (InstanceFinder.IsServerStarted)
        {
            ControlledShip controlledShip = other.gameObject.GetComponentInParent<ControlledShip>();

            if (controlledShip == null) {return;}
            powerupEffect.Apply(other.gameObject);

            // buffSpawner is a public class that all the powerups can access & talk to...
            // You need to pass in WHERE this powerup is, so that BuffSpawner knows that the spawnLocation 
            // is now free.

            buffSpawner.NotifyDespawn(SpawnPointOwner);
            SpawnPointOwner = null; // reset so fresh for next spawn
            ServerManager.Despawn(gameObject);


        }
    }
}