using System.Collections;
using FishNet.Managing.Server;
using FishNet.Object;
using UnityEngine;

public class BuffSpawner : NetworkBehaviour
{
    // Different buff types prefabs
    public GameObject healthBuffPrefab;
    public GameObject shieldBuffPrefab;

    // Buff spawn locations
    public GameObject buffSpawn1;
    public GameObject buffSpawn2;

    // Determine which spawnpoint is available/occupied
    private bool isBuffSpawn1_Occupied = false;
    private bool isBuffSpawn2_Occupied = false;

    // delay TODO change based on player.count
    private int delay = 15;

    public override void OnStartServer()
    {
        RandomBuffSelector(); // Call twice for 2 buffs?
        RandomBuffSelector(); // Check how many spawns there are, and run based on that.

        // Potentially SpawnBuffsInitial(); and then refactor spawn method to SpawnPooled(); 
    }

    private void RandomBuffSelector()
    {
        int randomBuffNumber = UnityEngine.Random.Range(1, 3);

        GameObject spawnPointOwner;  // = null; // Reset to null on start of method
        GameObject selectedBuffPrefab; // = null; // Reset to null on start of method

        spawnPointOwner = GetAvailableSpawnPoint();

        if (randomBuffNumber == 1) { selectedBuffPrefab = healthBuffPrefab; }
        else { selectedBuffPrefab = shieldBuffPrefab; }

        StartCoroutine(SpawnBuffWithDelay(spawnPointOwner.transform, spawnPointOwner, delay, selectedBuffPrefab ));
    }

    private GameObject GetAvailableSpawnPoint() // returns an available buffSpawn and sets bool.
    {
        if (!isBuffSpawn1_Occupied)
        {
            isBuffSpawn1_Occupied = true;
            return buffSpawn1;
        }
        
        if (!isBuffSpawn2_Occupied)
        {
            isBuffSpawn2_Occupied = true;
            return buffSpawn2;
        }

        // buffSpawn1 by default/failsafe
        return buffSpawn1;
    }

    private IEnumerator SpawnBuffWithDelay(Transform spawnLocation, GameObject spawnPointOwner, int delay, GameObject selectedBuffPrefab)
    {
        yield return new WaitForSeconds(delay);
        SpawnBuff(spawnLocation, spawnPointOwner, selectedBuffPrefab);
    }

    private void SpawnBuff(Transform spawnLocation, GameObject spawnPointOwner, GameObject selectedBuffPrefab)
    {
        NetworkObject nob = NetworkManager.GetPooledInstantiated(selectedBuffPrefab, spawnLocation.position, spawnLocation.rotation, IsServerStarted);
        ServerManager.Spawn(nob); 
        nob.GetComponent<Powerup>().SetSpawnPointOwner(spawnPointOwner);
    }

    public void NotifyDespawn(GameObject spawnPointOwner)
    {
        if (spawnPointOwner == buffSpawn1) 
        { 
            isBuffSpawn1_Occupied = false;
        }

        if (spawnPointOwner == buffSpawn2) 
        { 
            isBuffSpawn2_Occupied = false;
        }

        RandomBuffSelector();
    }
}