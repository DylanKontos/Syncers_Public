using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class BotManager : NetworkBehaviour
{

    BotSpawner botSpawner;
    PlayerManager playerManager;
    
    // PlayerManager also has a botCount and AllBots list.
    // TODO: refactor...
    public readonly SyncVar<int> botCount = new();
    readonly SyncList<NetworkObject> AllBots = new SyncList<NetworkObject>(); // keep a list of networkobjects bots

    // Make public for testers
    private int desiredBots = 1;
    private bool despawnBotsIfMoreThanOnePlayer = true;

    private void Start()
    {
        botSpawner = GetComponent<BotSpawner>();
        playerManager = FindFirstObjectByType<PlayerManager>();
    }

    private void Update()
    {
        if (!InstanceFinder.IsServerStarted) return;

        if (playerManager.playerCount.Value >= 2 && despawnBotsIfMoreThanOnePlayer == true) 
        { 
            DespawnAllBots();
        }

        else
        {
            SpawnBot();
        }
    }

    public void DespawnBot(NetworkObject bot)
    {
        if (!IsServerInitialized) return;
        
        if (bot != null)
        {
            bot.Despawn();
            AllBots.Remove(bot);
            botCount.Value -= 1;
        }

        else // failsafe incase bot wont despawn.
        {
            Debug.Log("Bot was null, so DespawnAllBots()");
            DespawnAllBots();
        }
    }
    
    public void DespawnAllBots()
    {
        botSpawner.DespawnAllBots();
        AllBots.Clear();
        botCount.Value = 0;
    }

    private void SpawnBot()
    {
        if (botCount.Value == desiredBots) return;

        botSpawner.SpawnBot();
        botCount.Value += 1;
    }

}
