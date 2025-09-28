using FishNet.Object; 
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using System.Collections.Generic;
using UnityEngine;
// WinterBolt using GameManager (same thing) as a singelton and sealed class:
// public sealed class
// public static GameManager Instance { get; private set; }
// [SyncObject] public readonly SyncList<Player> players = new();  VERSION 4 NOT NEEDED!

// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// TO:DO this logic to run every time a player join/leaves // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// TO:DO Change scoreboard logic to pull data from here.... potentially.


public class PlayerManager : NetworkBehaviour   // Aim - A public list of all player singeltons, public access for camera targetting.
{

    public readonly SyncList<NetworkObject> AllPlayers = new SyncList<NetworkObject>();
    public readonly SyncList<NetworkObject> AllControlledShips = new SyncList<NetworkObject>();
    public readonly SyncList<NetworkObject> AllBots = new SyncList<NetworkObject>(); // doubled in BotManager TODO REFACTOR
    // Bot manager will spawn/despawn bots. PlayerManager just keeps track and some access from BotManager

    public readonly SyncVar<int> playerCount = new();
    public readonly SyncVar<int> controlledShipCount = new();
    public readonly SyncVar<int> botCount = new(); // doubled in BotManager TODO REFACTOR



    public void Update()   
    {
        AddPlayer();
        AddControlledShip();
        // AddBot(); // Camera relies on a ControlledShip // Less code to pass Bots in as controlledShips
    }

    private void AddPlayer()
    {
        if (!IsServerInitialized) return;  // Important // do not change a [SyncObject] as Client

        AllPlayers.Clear();     // Clear the list to avoid duplicates   // // TODO this logic to run every time a player join/leaves

        foreach (KeyValuePair<int, NetworkConnection> kvp in ServerManager.Clients) // Iterate over the NetworkConnections
        {
            foreach (NetworkObject networkObject in kvp.Value.Objects)    // Iterate over the NetworkObjects for each NetworkConnection
            {
                if (networkObject.name == "Player(Clone)") // Check if the NetworkObject is a Player(Clone)
                {
                    Player player = networkObject.GetComponent<Player>(); // Get the Player component
                    // bool isPlayerAlive = player.isAlive.Value;

                    if (player.isAlive.Value == true)
                    {
                        AllPlayers.Add(networkObject); // Add to list
                    }
                }
            }
        }
        // Get the count of players
        playerCount.Value = AllPlayers.Count;
    }

    private void AddControlledShip()
    {
        if (!IsServerInitialized) return; // Important // do not change a [SyncObject] as Client
        
        AllControlledShips.Clear();     // Clear the list to avoid duplicates   // // TODO this logic to run every time a player join/leaves
        
        foreach (KeyValuePair<int, NetworkConnection> kvp in ServerManager.Clients) // Iterate over the NetworkConnections
        {
            foreach (NetworkObject networkObject in kvp.Value.Objects)    // Iterate over the NetworkObjects for each NetworkConnection
            {
                if (networkObject.name == "ShipPrefab(Clone)") // Check if the NetworkObject is a ControlledShip
                {
                    AllControlledShips.Add(networkObject); // Add to list
                }
            }
        }
        
        // Add bots to ControlledShip a camera relies on this // Check camera logic before considering an AllBots SyncList >.<  ....
        NetworkObject[] botNetworkObjects = Object.FindObjectsOfType<NetworkObject>();
        foreach (NetworkObject botNetworkObject in botNetworkObjects)
        {
            if (botNetworkObject.name == "BotPrefab(Clone)")
            {
                AllControlledShips.Add(botNetworkObject); // Add to list
                // WILL SPAM IN SERVER
            }
        }
        controlledShipCount.Value = AllControlledShips.Count;
    }
}

// private void AddBot()
// {
//     AllBots.Clear();     // Clear the list to avoid duplicates   // // TODO this logic to run every time a player join/leaves
//     
//     // Add bot ships to the list of controlled ships
//     NetworkObject[] botNetworkObjects = Object.FindObjectsOfType<NetworkObject>();
//     foreach (NetworkObject botNetworkObject in botNetworkObjects)
//     {
//         if (botNetworkObject.name == "BotPrefab(Clone)")
//         {
//             AllBots.Add(botNetworkObject); // Add to list
//             // WILL SPAM IN SERVER
//         }
//     }
//     botCount.Value = AllBots.Count;
// }

    // foreach (NetworkObject player in AllPlayers)
    // {
    // }