using FishNet.Object;
using UnityEngine;

public class BotSpawner : NetworkBehaviour
{
    public GameObject botPrefab;

    public void SpawnBot()
    {
        if (!IsServerInitialized) return;

        NetworkObject nob = NetworkManager.GetPooledInstantiated(botPrefab, IsServerStarted);
        // nob.transform.position = transform.position;
        // nob.transform.rotation = transform.rotation;
        ServerManager.Spawn(nob, Owner);
    }

    public void DespawnAllBots()
    {
        if (!IsServerInitialized) return;

        var objectsToDespawn = GameObject.FindObjectsOfType<NetworkObject>();
        foreach (var obj in objectsToDespawn)
        {
            if (obj.gameObject.name == "BotPrefab(Clone)")
            {
                obj.Despawn();
            }
        }
    }
}
