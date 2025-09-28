using UnityEngine;
using FishNet.Object; 
using FishNet.Object.Synchronizing;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System.Collections;
using FishNet.Component.Utility;
using FishNet.Managing.Timing;


public sealed class Player : NetworkBehaviour    // all players spawn with a player object that has this script. It then assigns a controlled ship
// Example Player > ControlledShip > PlayerObjectWeapons > Predicted Projectile
{
    //PlayerInput playerInput; // this is Unity's action map playerInput

    public static Player Instance { get; private set; }

    public string PreviousName { get; private set; }

    NetworkManager networkManager;
    Tugboat tugboat;
    TimeManager timeManager;
    ScoreboardDisplayer scoreboardDisplayer;
    RespawnMenuDisplayer respawnMenuDisplayer;
    PlayerManager playerManager;
    PlayerObjectMovement playerObjectMovement;
    XsollaPlayerDataManager xsollaPlayerDataManager;
    PingDisplay pingDisplay;
    


    private string serverAddress;

    // public string selectedSkinPlayer { public get; private set; }

    public string selectedSkinPlayer = "freighter"; // default free skin is freighter...

    public readonly SyncVar<string> playerSkin = new();
    public readonly SyncVar<ControlledShip> controlledShip = new();  // references the specific ship conrolled by the client/player  // firstobjectnotifier also does this
    public readonly SyncVar<string> playerName = new();
    public readonly SyncVar<int> kills = new();
    public readonly SyncVar<int> deaths = new();
    public readonly SyncVar<bool> isAlive = new();
    public readonly SyncVar<bool> isReady = new();
    public readonly SyncVar<Team> playerTeam = new SyncVar<Team>(Team.Spectators);   // enum below
    public readonly SyncVar<NetworkConnection> networkConnection = new();
    public readonly SyncVar<long> serverClientPing = new();

    // public NetworkConnection networkConnection; // = new NetworkConnection();
    
    public enum Team
    {
        Red,
        Blue,
        Spectators
    }

    public GameObject shipPrefab;
    public GameObject spectatorPrefab;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            // Debug.Log("Client" + LocalConnection);
		    Instance = this;
            scoreboardDisplayer = FindObjectOfType<ScoreboardDisplayer>();
            respawnMenuDisplayer = FindObjectOfType<RespawnMenuDisplayer>();
            networkManager = FindObjectOfType<NetworkManager>(); // I think I can use InstanceFinder.NetworkNanager();

            xsollaPlayerDataManager = networkManager.GetComponent<XsollaPlayerDataManager>();

            pingDisplay = FindObjectOfType<PingDisplay>();
            tugboat = networkManager.GetComponent<Tugboat>(); 
            serverAddress = tugboat.GetClientAddress();

            SetPlayerSkinStartup();
            SetDisplayNameStartup(); // Will take Xsolla username OR just a player# from FishNet // TRY ON SERVER?
            PingServer(base.Owner); 
        }
    }

    public override void OnStartServer()
    {
        if (!IsServerInitialized) return;

        xsollaPlayerDataManager = FindFirstObjectByType<XsollaPlayerDataManager>();

        // networkConnection.Value = InstanceFinder.ClientManager.Connection;
        networkConnection.Value = LocalConnection;
        // SetAlive();
        controlledShip.Value = null; 
        playerTeam.Value = Team.Spectators;
    }



    public void SetPlayerSkinStartup()
    {
        if (!IsOwner) return;


        // Determine the skin, if null/empty, use default skin = Freighter
        string selectedSkinPlayer = !string.IsNullOrEmpty(xsollaPlayerDataManager.selectedSkin)
            ? xsollaPlayerDataManager.selectedSkin
            : $"freighter";

        SetSkin(selectedSkinPlayer);
    }

    public void SetDisplayNameStartup()
    {
        if(!IsOwner) return;

        // Determine the display name
        string displayName = !string.IsNullOrEmpty(xsollaPlayerDataManager.displayName)
            ? xsollaPlayerDataManager.displayName
            : $"Player#{base.ObjectId}";

        // Call the ServerRpc to set the display name on the server
        SetName(displayName); // This sets it on player, but not on the canvas of the ship.
    }

    // Steam Name Setup bugged for clients && in webgl not running...
    // public void SetDisplayNameStartup()
    // {
    //     if(!IsOwner) return;

    //     Debug.Log("SetDisplayNameStartup");

    //     string displayName = "Player#" + base.ObjectId; // Initialize/default displayName

    //     if (steamNameSetter !=null && !string.IsNullOrEmpty(steamNameSetter.steamName)) 
    //     {
    //         displayName = steamNameSetter.steamName;
    //         Debug.Log("steamNameSetter !=null " + displayName);
    //     }

    //     // I only want 1 condition to execute, therefore I will use else if here
    //     // Alternatively change to if to make xsolla names override steam names?
    //     if (xsollaPlayerDataManager !=null && !string.IsNullOrEmpty(xsollaPlayerDataManager.displayName))
    //     {
    //         displayName = xsollaPlayerDataManager.displayName;
    //         Debug.Log("xsollaPlayerDataManager !=null " + displayName);
    //     }

    //     // Call the ServerRpc to set the display name on the server
    //     SetName(displayName); // This sets it on player, but not on the canvas of the ship.
    // }

    // [TargetRpc]
    private void PingServer(NetworkConnection conn)
    {
        StartCoroutine(PingCoroutine());
    }

    IEnumerator PingCoroutine()
    {
        while (true)
        {
            long ping; // locally set ping
            TimeManager tm = InstanceFinder.TimeManager;

            if (tm == null)
            {
                ping = 0;
            }

            else
            {
                ping = tm.RoundTripTime;
                SetSyncvarClientPing(ping); // pass into a ServerRpc
                // serverClientPing.Value = ping; // CARE CHANGING SYNCVAR ON CLIENT
            } 
            yield return new WaitForSeconds(5f); // Ping every 5 seconds
        }
    }

    [ServerRpc]
    private void SetSyncvarClientPing(long ping)
    {
        serverClientPing.Value = ping; // Change the SyncVar on the server
    }
    
    public override void OnStopServer() // Used to remove scoreboard entries
    {
        base.OnStopServer();

        scoreboardDisplayer = FindObjectOfType<ScoreboardDisplayer>();
        scoreboardDisplayer.RemoveScoreboardEntry(ObjectId); // Error on stop?
        playerManager = FindObjectOfType<PlayerManager>(); 
    }

    public override void OnStopClient()   
    {
        base.OnStopClient();  // Lesson - Client is disconnected and can't remove any entries! Do it in Server!
        Destroy(gameObject); // Qustion - Do I need to destroy this Player GameObject?
    }



    public static bool GetScoreboardButtonState()   // static!
    {
        if (Instance != null && Instance.controlledShip.Value != null)
        {
            return Instance.controlledShip.Value.playerObjectInput.inputStates.DisplayScoreboardButton;
        }
      return false;
    }

    public void SetNameControlledShip(string newName) // Sets the name specifically (not static), used on death
    {                                                 // hence the ownership check....
        if (!IsOwner) return;

        if (newName == null)
        {
            newName = Instance.PreviousName;  // Use the previous name
        }
        
        Instance.ServerSetName(newName);  
    }


    public static void SetName(string newName)  // called by client // STATIC
{
    if (newName == null)
    {
        newName = Instance.PreviousName;  // Use the previous name
    }
    
    else
    {
        Instance.PreviousName = newName;  // Update previous name
    }
    
    Instance.ServerSetName(newName);
}


    public void SetSkin(string selectedSkinPlayer)
    {
        Instance.ServerSetSkin(selectedSkinPlayer);
    }

    public void SetAlive()
    {
        if (!IsServerInitialized) return; 
        isAlive.Value = true;
    }

    public void SetDead()
    {
        if (!IsServerInitialized) return; 
        isAlive.Value = false;
        EnableDeathScreenTargetRpc(base.Owner);
    }

    [TargetRpc]
    private void EnableDeathScreenTargetRpc(NetworkConnection conn)
    {
        respawnMenuDisplayer.DeathScreenEnable();
    }

    [ServerRpc]
    public void ServerSetSkin(string selectedSkinPlayer)
    {
        if (!IsServerInitialized) return;  

        playerSkin.Value = selectedSkinPlayer;
    }

    [ServerRpc]
    public void ServerSetName(string newName)  // client asks server to set name   // and also scoreboard
    {
        if (!IsServerInitialized) return;  

        playerName.Value = newName;  
        
        if (controlledShip.Value != null) 
        {
            controlledShip.Value.nameDisplayer.SetName(playerName.Value); 
        }
        else 
        {
            StartCoroutine(CheckControlledShipAndSetName());
        }

        // if (controlledShip.Value != null) { controlledShip.Value.nameDisplayer.SetName(playerName.Value); } // set name for client
        
        scoreboardDisplayer = FindObjectOfType<ScoreboardDisplayer>();
        scoreboardDisplayer.AddScoreboardEntry(playerName.Value, kills.Value, deaths.Value, ObjectId, serverClientPing.Value);
    }

    private IEnumerator CheckControlledShipAndSetName()
    {
        while (controlledShip.Value == null) 
        {
            yield return new WaitForSeconds(1); // Check every second
        }
        
        controlledShip.Value.nameDisplayer.SetName(playerName.Value); // Execute the code when controlledShip is not null
    }

    [ServerRpc]
    public void SetPlayerTeam(Team team) // red/blue/spectators
    {
        playerTeam.Value = team; // pass in from TeamSelectMenu

        if (playerTeam.Value != Team.Spectators)
        {
            TryRespawn(); // You are not the Owner of this Object
        }

        else
        {
            TryRespawnSpectator();
        }
    }
 
	// Called from ServerRpc // This is running on the server
	public void TryRespawn()   // Called from RespawnMenu //  // Called from SetPlayerTeam
	{
		// if (!IsServerInitialized) return;  	   // If deleted, no change. 

        // Doesn't work.....
        // If you're already alive, return!
        // if (isAlive.Value == true) return;

		GameObject redSpawn = GameObject.Find("Spawn_Red");          // Find Spawn Points
        GameObject blueSpawn = GameObject.Find("Spawn_Blue");        // Find Spawn Points

        if (controlledShip.Value == null && playerTeam.Value == Team.Red)         // if not spawned yet, and Red team
        {           
            GameObject redShipInstance = Instantiate(shipPrefab, redSpawn.transform.position, Quaternion.Euler(0f, redSpawn.transform.eulerAngles.y, 0f) ); 
            InstanceFinder.ServerManager.Spawn(redShipInstance, Owner);  
            controlledShip.Value = redShipInstance.GetComponent<ControlledShip>();
            controlledShip.Value.player.Value = this;   // Used in OnDeath of ControlledShip
            
            controlledShip.Value.AssignMeshRenderer(playerTeam.Value, playerSkin.Value); // TODO: RENAME  - ALSO ASSIGNS VFX
        }

        if (controlledShip.Value == null && playerTeam.Value == Team.Blue)
        {
            GameObject blueShipInstance = Instantiate(shipPrefab, blueSpawn.transform.position, Quaternion.Euler(0f, blueSpawn.transform.eulerAngles.y, 0f) ); 
            InstanceFinder.ServerManager.Spawn(blueShipInstance, Owner);  
            controlledShip.Value = blueShipInstance.GetComponent<ControlledShip>();
            controlledShip.Value.player.Value = this; 

            controlledShip.Value.AssignMeshRenderer(playerTeam.Value, playerSkin.Value); // TODO: RENAME  - ALSO ASSIGNS VFX
        }

        if (controlledShip.Value != null && playerTeam.Value == Team.Red)   // if spawned once before && and red team
        {
            controlledShip.Value.transform.position = redSpawn.transform.position;   // TODO: Change to instantiation manner of 
            controlledShip.Value.transform.rotation = redSpawn.transform.rotation;
            ClearReplicateCache();
        }
        
        if (controlledShip.Value != null && playerTeam.Value == Team.Blue)
        {
            controlledShip.Value.transform.position = blueSpawn.transform.position;
            controlledShip.Value.transform.rotation = blueSpawn.transform.rotation;
			ClearReplicateCache();
		}

		controlledShip.Value.meshRenderer.enabled = true;   
		controlledShip.Value.playerInput.enabled = true;  
		controlledShip.Value.boxCollider.enabled = true;

		SetAlive();
		controlledShip.Value.RestoreHealth();

        // // RESETTING LASER CAMERA TO PREVENT BUG
        // PlayerObjectWeapons playerObjcetWeapons = controlledShip.Value.GetComponent<PlayerObjectWeapons>();
        // playerObjcetWeapons.ResetLaserCamera();

        SetNameControlledShip(playerName.Value);

	}

    [ServerRpc]
	public void TryRespawnServerRpc()   // Called from RespawnMenu
	{        
        TryRespawn(); // Will exectue on server
        EnableShip();
	}

    [ObserversRpc(BufferLast = true, ExcludeOwner = false)]
    public void EnableShip()
    {
        controlledShip.Value.meshRenderer.enabled = true; // Changes based on team, needs a SyncVar // BUG Doesn't exist for late joiners.... 
        controlledShip.Value.playerInput.enabled = true;   // Never changes
		controlledShip.Value.boxCollider.enabled = true;    // Never changes
    }

    private void TryRespawnSpectator()
    {
        GameObject redSpawn = GameObject.Find("Spawn_Red");          // Find Spawn Points
        GameObject blueSpawn = GameObject.Find("Spawn_Blue");        // Find Spawn Points

        GameObject spectatorInstance = Instantiate(spectatorPrefab, blueSpawn.transform.position,  Quaternion.Euler(0f, redSpawn.transform.eulerAngles.y, 0f));
        InstanceFinder.ServerManager.Spawn(spectatorInstance, Owner); 
    }

    public void AddKill()
    {
        if (!IsServerInitialized) return; // SERVER CHECK

        // Debug.Log("Player - AddKill()");

        kills.Value += 1; 

        if (xsollaPlayerDataManager.isLoggedIn)
        {
            AddKillTargetRpc(base.Owner);
        }

        SetName(null); // update scoreboard


        // SetNameControlledShip(null);
        // SetName(null);

    }

    [TargetRpc]
    private void AddKillTargetRpc(NetworkConnection conn)
    {
        xsollaPlayerDataManager.AddKill();
    }

}
    

    // Maxx Kratt - === TODO CALLING FROM RESPAWNMENU 22/11/2023// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // I would seperate the TryRespawn code into a regular method and then have an
    // optional ServerRpc that can call it
    // That way the server can use the regular one and clients can use the ServerRpc
    // DONE!

    // public static NetworkObject GetFromID(int id, NetworkManager networkManager = null)
    // {
    //     networkManager ??= InstanceFinder.NetworkManager;
    //     var spawnedObjects = networkManager.IsServer ? networkManager.ServerManager.Objects.Spawned : networkManager.ClientManager.Objects.Spawned;
    //     spawnedObjects.TryGetValue(id, out NetworkObject networkObject);

    //     return networkObject;
    // }


    // Start of IP/Leaderboard Chain
    // private void FetchPlayerIp() // Called OnStartServer(); // So.. Maybe a save very kill? // TODO TODO TODO
    // {
    //     string connectionInfo = networkConnection.Value.ToString(); 
    //     string[] parts = connectionInfo.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
    //     string playerIP = parts.Length > 3 ? parts[3] : "IP not found";

    //     if (xsollaPlayerDataManager.displayName == null) return;

    //     if (playerIP != null) { xsollaPlayerDataManager.SetLeaderboardEntry(playerIP); } 
    // }

    // private void FetchPlayerIp(NetworkConnection conn)
    // {
    //     string firstConnectionInfo = networkConnection.Value.ToString();
    //     StartCoroutine(FetchPlayerIpCoroutine());
    // }

    // private IEnumerator FetchPlayerIpCoroutine()
    // {        
    //     string playerIP = "IP not found";

    //     while (playerIP == "IP not found")
    //     {
    //         string connectionInfo = networkConnection.Value.ToString(); 
    //         string[] parts = connectionInfo.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
    //         playerIP = parts.Length > 3 ? parts[3] : "IP not found";
            
    //         if (playerIP != "IP not found" || playerIP != "Unset" || playerIP != "unset")
    //         {
    //             if (xsollaPlayerDataManager.displayName == null)
    //             {
    //                 yield break;
    //             }

    //             if (playerIP != null) 
    //             { 
    //                 xsollaPlayerDataManager.SetLeaderboardEntry(playerIP); 
    //             }

    //             yield break; // Exit the coroutine once IP is found
    //         }
            
    //         yield return new WaitForSeconds(5); // Retry every 10 seconds
    //     }
    // }