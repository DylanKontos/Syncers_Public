using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object; 
using FishNet.Object.Synchronizing;
using System;
using FishNet.Connection;
using FishNet.Component.Prediction;
using Syncers.Interfaces;
using UnityEngine.UI; // Import the namespace where IDamageable is defined



public sealed class ControlledShip : NetworkBehaviour, IDamageable
{
    public readonly SyncVar<Player> player = new();  // which player is controlling the ship // contains name
    // When this value changes on the server, it will be snyced on the client. CHANGE ON SERVER ONLY (Client won't work)
	public readonly SyncVar<int> health = new SyncVar<int>(100);
	public readonly SyncVar<double> sync = new SyncVar<double>(100.0);
	public readonly SyncVar<string> controlledShipID = new SyncVar<string>();
	public readonly SyncVar<bool> isShieldActive = new SyncVar<bool>(false);

	private NetworkCollision _networkCollision;
	UiDisabler uiDisabler;

    float regenerationRate = 0.1f;
    double maxSync = 100; 

	public ParticleSystem DeathExplosion;
	public ParticleSystem HitExplosion;
	public ParticleSystem shieldFX;

	public Material redMaterial;
	public Material blueMaterial;

	public GameObject nameDisplayCanvas;
	private LookAtCamera lookAtCamera;

	//public Transform tf;
	public GameObject GraphicalObject;
	public GameObject Hitbox;

	public PlayerInput playerInput;		// !! ALERT !! Made public for player to call in respawn
	public BoxCollider boxCollider;      // !! ALERT !! Made public for player to call in respawn
	public NameDisplayer nameDisplayer;	
	public HealthBar healthBar;	
	private NameSetterCanvas nameSetterCanvas;
	public PlayerObjectInput playerObjectInput;
	private PlayerObjectMovement playerObjectMovement;

	public GameObject meshRendererParent;
	public MeshRenderer meshRenderer; // This needs to be a syncvar if player is accessing it...
	public MeshRenderer meshRendererChildStriker;
	public MeshRenderer meshRendererChildFreigther;
	public MeshRenderer meshRendererChildHunter;
	public MeshRenderer meshRendererChildSpectre;

	public Image healthBarFill;
	

	public void Start()
    {
		playerObjectInput = GetComponent<PlayerObjectInput>();
		nameSetterCanvas = FindObjectOfType<NameSetterCanvas>(); // This is disabled.
    }

	public override void OnStartServer()
	{
		base.OnStartServer();
		RestoreHealth(); // Restore Helth is run on server, Therefore must be here NOT in OnStartClient()
		controlledShipID.Value = $"controlledShipID [ID:{base.ObjectId}]";
		healthBar = GetComponentInChildren<HealthBar>();
	}

	public override void OnStartClient()
	{
		base.OnStartClient(); 	// Always call base method first ( bugs otherwise)
		GraphicalObject = transform.Find("GraphicalObject").gameObject;    // Care, potential issue in respawns
		// meshRenderer = GraphicalObject.GetComponentInChildren<MeshRenderer>();  // Changed to AssignMeshRenderer
		playerInput = GetComponent<PlayerInput>(); 
		nameDisplayer = GetComponentInChildren<NameDisplayer>(); // Display Name above Ship
		playerObjectInput = GetComponent<PlayerObjectInput>();
		uiDisabler = GetComponent<UiDisabler>();
	}

	// called from server
	public void AssignMeshRenderer(Player.Team team, string playerSkin) // Called from player on spawn
	{
		playerObjectMovement = GetComponent<PlayerObjectMovement>();

		if (team == Player.Team.Red)
		{
			if (playerSkin == null || playerSkin == "freighter") // playerSkin == null == NO-LOGIN
			{
				meshRenderer = meshRendererChildFreigther;
				meshRenderer.material = redMaterial;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXFreighter();
			}

			if (playerSkin == "striker")
			{
				meshRenderer = meshRendererChildStriker;
				Material[] materials = meshRenderer.materials;
				materials[0] = redMaterial;
				materials[2] = redMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXStriker();
			}

			if (playerSkin == "hunter")
			{
				meshRenderer = meshRendererChildHunter;
				Material[] materials = meshRenderer.materials;
				materials[0] = redMaterial;
				materials[1] = redMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXHunter();
			}

			if (playerSkin == "spectre")
			{
				meshRenderer = meshRendererChildSpectre;
				Material[] materials = meshRenderer.materials;
				materials[0] = redMaterial;
				materials[1] = redMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXSpectre();
			}
		}
			
		if (team == Player.Team.Blue)
		{
			healthBarFill.color = Color.blue;
			
			if (playerSkin == null || playerSkin == "freighter") // playerSkin == null == NO-LOGIN
			{
				meshRenderer = meshRendererChildFreigther;
				meshRenderer.material = blueMaterial;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXFreighter();
			}
			if (playerSkin == "striker")
			{
				meshRenderer = meshRendererChildStriker;
				Material[] materials = meshRenderer.materials;
				materials[0] = blueMaterial;
				materials[2] = blueMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXStriker();
			}

			if (playerSkin == "hunter")
			{
				meshRenderer = meshRendererChildHunter;
				Material[] materials = meshRenderer.materials;
				materials[0] = blueMaterial;
				materials[1] = blueMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXHunter();
			}

			if (playerSkin == "spectre")
			{
				meshRenderer = meshRendererChildSpectre;
				Material[] materials = meshRenderer.materials;
				materials[0] = blueMaterial;
				materials[1] = blueMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXSpectre();
			}
		}

		AssignMeshRendererObserversRpc(team, playerSkin);
	}


	// Observers RPC // // Observers RPC // 
	[ObserversRpc (BufferLast = true)] // ExcludeOwner = false)]
	private void AssignMeshRendererObserversRpc(Player.Team team, string playerSkin)
	{
		playerObjectMovement = GetComponent<PlayerObjectMovement>();
			
		if (team == Player.Team.Red)
		{
			if (playerSkin == null || playerSkin == "freighter")
			{
				meshRenderer = meshRendererChildFreigther;
				meshRenderer.material = redMaterial;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXFreighter();
			}

			if (playerSkin == "striker")
			{
				meshRenderer = meshRendererChildStriker;
				Material[] materials = meshRenderer.materials;
				materials[0] = redMaterial;
				materials[2] = redMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXStriker();
			}

			if (playerSkin == "hunter")
			{
				meshRenderer = meshRendererChildHunter;
				Material[] materials = meshRenderer.materials;
				materials[0] = redMaterial;
				materials[1] = redMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXHunter();
			}

			if (playerSkin == "spectre")
			{
				meshRenderer = meshRendererChildSpectre;
				Material[] materials = meshRenderer.materials;
				materials[0] = redMaterial;
				materials[1] = redMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXSpectre();
			}
		}
			
		if (team == Player.Team.Blue)
		{
			healthBarFill.color = Color.blue;
			
			if (playerSkin == null || playerSkin == "freighter")
			{
				meshRenderer = meshRendererChildFreigther;
				meshRenderer.material = blueMaterial;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXFreighter();
			}
			if (playerSkin == "striker")
			{
				meshRenderer = meshRendererChildStriker;
				Material[] materials = meshRenderer.materials;
				materials[0] = blueMaterial;
				materials[2] = blueMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXStriker();
			}

			if (playerSkin == "hunter")
			{
				meshRenderer = meshRendererChildHunter;
				Material[] materials = meshRenderer.materials;
				materials[0] = blueMaterial;
				materials[1] = blueMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXHunter();
			}

			if (playerSkin == "spectre")
			{
				meshRenderer = meshRendererChildSpectre;
				Material[] materials = meshRenderer.materials;
				materials[0] = blueMaterial;
				materials[1] = blueMaterial;
				meshRenderer.materials = materials;
				meshRenderer.enabled = true;
				playerObjectMovement.SetPlayerVFXSpectre();
			}
		}
	}
	
	public override void OnStopClient()
	{
		base.OnStopClient();
		Destroy(gameObject); // DESTROYING Game Object
	}

	public void Awake()
	{
        _networkCollision = boxCollider.GetComponent<NetworkCollision>();
		_networkCollision.OnEnter += NetworkCollisionEnter; // Subscribe to the desired collision event
		RestoreHealth();
	}

	public void OnDisable()
	{
		_networkCollision.OnEnter -= NetworkCollisionEnter;
	}


	#region NETCOLLISION
    #endregion NETCOLLISION


	// The position of projectiles / playerObjects is all predicted, so in theory this can only ever be called
	// by the server... Server check anyway...
	private void NetworkCollisionEnter(Collider other)
	{
		if (!IsServerInitialized) { return; }

		// TestProjectile testProjectile = other.GetComponent<TestProjectile>();
		// Player ownerPlayer = null;

		// if (testProjectile != null)
		// {
		// 	int damage = testProjectile.damage;
		// 	testProjectile.DisableProjectile();

		// 	if (testProjectile.projectileOwnerPlayer.Value !=null )
		// 	{
        //     	ownerPlayer = testProjectile.projectileOwnerPlayer.Value;
		// 	}

        //     ReceiveDamage(damage, ownerPlayer != null ? ownerPlayer : null);
		// 	testProjectile.DisableProjectile(); // !!! DISABLE THE PROJECTILE !!!

		// 	// Player cannot use this // It will cause BUG with Bot projectiles.
        //     // ownerPlayer = testProjectile.Owner.FirstObject.GetComponent<Player>();

		// }
	}


	#region TakeDamage
    #endregion TakeDamage

	public void TakeDamage(DamageData damageData)
    {
		if (!IsServerInitialized) return; // If the server is not active, return;
		ReceiveDamage(damageData.damageAmount, damageData.damageSourcePlayer);
    }


	#region ReceiveDamage
    #endregion ReceiveDamage

	// ServerOnly Method
	public void ReceiveDamage(int amount, Player killerPlayer, NetworkConnection networkConnection = null) // pass in damage and player who created projectile that hit this ship.
	{
		if (!IsServerInitialized) return; // If the server is not active, return;
		// if (player.Value.isAlive.Value == false) return; // if THIS player is not alive. return;

		// Debug.Log(killerPlayer);

		if (health.Value > 0)
		{
			if (isShieldActive.Value == true)  // shield check
			{		
				DeactivateShield(); 
				return;
			}

			health.Value -= amount;
			PlayHitExplosion();

			if (healthBar != null)
			{
				healthBar.TakeDamage(amount);
			}

			if (health.Value <= 0) // could add in a player.IsAlive check.... 20/03
			{
				// Will pass in killer player if not null, otherwise, passes in OnDeath(null);
        		// OnDeath(killerPlayer != null ? killerPlayer : null); // causing bug on death...

				// OnDeath();

				if (killerPlayer != null) { OnDeath(killerPlayer); }
				else { OnDeath(null); }
			}
		}

		// FailSafe...
		else // else if (health.Value <= 0)  // else if (IsAlive) 
        {
        	// OnDeath(killerPlayer != null ? killerPlayer : null);
			// OnDeath(null);
			OnDeath(null);
        }
	}

	

	public void RestoreHealth()
	{
		if (!IsServerInitialized) return;

		health.Value = 100;
		sync.Value = 100;

		if (healthBar != null ) { 		healthBar.ResetHealthBar(); }
	}

	public void AddShield() // If projectiles could rebound off this, that would be awesome...
	{
		if (!IsServerInitialized) return;
	
		isShieldActive.Value = true;
		ActivateShieldFX();
	}

	private void DeactivateShield() 
	{
		isShieldActive.Value = false;
		DeactivateShieldFX();
		// DeActivateShieldFXTargetRpc(base.Owner);
	}

	// private void OnDeath(Player killerPlayer)  
	private void OnDeath(Player killerPlayer)  
	{
		if (!IsServerInitialized) return; // SERVER CHECK

		if (killerPlayer != null) 
		{ 
			killerPlayer.AddKill();
		}

		// DeathExplosion.Play();

		meshRenderer.enabled = false;  
		playerInput.enabled = false;    
		boxCollider.enabled = false;

		player.Value.SetDead();
		player.Value.deaths.Value += 1;



		OnDeathObserversRpc(null);

		// OnDeathObserversRpc(killerPlayer);
	}

	// [ObserversRpc(ExcludeOwner = false, BufferLast = true)]
	// private void AddKillerPlayerKill(Player killerPlayer)
	// {
	// 	killerPlayer.kills.Value += 1;
	// }

	[ObserversRpc(ExcludeOwner = false, BufferLast = true)]   
	private void OnDeathObserversRpc(Player killerPlayer)                 
	{
		DeathExplosion.Play();

		meshRenderer.enabled = false; 
		playerInput.enabled = false;   
		boxCollider.enabled = false; 

		player.Value.SetNameControlledShip(player.Value.playerName.Value);

		if (killerPlayer != null) { killerPlayer.SetNameControlledShip(null); }

	}

	[ObserversRpc(ExcludeOwner = false)]   
	private void PlayHitExplosion()
	{
		HitExplosion.Play();
	}

	[ObserversRpc(ExcludeOwner = false)]  
	private void ActivateShieldFX()
	{
        shieldFX.Play();
	}

	[ObserversRpc(ExcludeOwner = false)]   
	private void DeactivateShieldFX()
	{
        shieldFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);    
	}

	public void ReduceSync() // TODO 
	{
		if (!IsServerInitialized) return;

		sync.Value -= 0.6f;  // was 0.5, 0.6 now for WebGL  TO:DO !!! WHAT IS GOING ON WITH VARIABLES
	}

	public void FreezeSync()
    {
        if (!IsServerInitialized) return;
		
        sync.Value -= 0.2f;  // Not working in WebGL builds. Doesn't actually freeze Sync // trying 0.2f instead... was 0.1f
    }

	public void RegenerateSync() // Called every tick, no need for Time.detlaTime;
	{
		if (!IsServerInitialized) return;

		// Calculate the boost to add this !>FRAME<! based on the regeneration rate.
		//double boostToAdd = regenerationRate * Time.deltaTime;   // NO...
		// Add the boost to the current value (controlledShip.boost) but limit it to not exceed the maximum boost value.

		sync.Value = Math.Min(sync.Value + regenerationRate, maxSync);   // Math.Min is FASTER in WebGL...
		
	}

	public void SetTargetCamNameDisplayer() 
	{
		lookAtCamera = nameDisplayCanvas.GetComponent<LookAtCamera>();
		lookAtCamera.StopCoroutine("LookAtTargetCam"); // Stop the other coroutine
    	lookAtCamera.StartCoroutine("LookAtMainCam"); // Start coroutine LookAtMainCam
	}

	public void SetFreeLookCamNameDisplayer()
	{
		lookAtCamera = nameDisplayCanvas.GetComponent<LookAtCamera>();
    	lookAtCamera.StopCoroutine("LookAtMainCam"); // Stop the other coroutine
    	lookAtCamera.StartCoroutine("LookAtTargetCam"); // Start coroutine LookAtTargetCam
	}
}


	// (NetworkConnection networkConnection = null) parameter will - 
	// This will automatically pass in all the networkConnections that run the code.
	// Use this against the networkConnection I WANT to run the code (player)
	// TODO REMOVE SERVER RPC, You're calling this on the server anyway in OnNetworkCollision
	// [ServerRpc(RequireOwnership=false)]
	// public void ReceiveDamageServerRpc(int amount, Player player, NetworkConnection networkConnection = null) // pass in damage and player who created projectile that hit this ship.
	// {
	// 	// if (player.networkConnection.Value.ClientId != networkConnection.ClientId) return;
	// 	// Only subtract damage if health is greater than 0

	// 	bool shouldApplyDamage = true; // Flag to control the flow

	// 	if (isShieldActive.Value == true) 
	// 	{		
	// 		DeactivateShield();   
	// 		isShieldActive.Value = false;
	// 		shouldApplyDamage = false; // Prevent applying damage
	// 	} // shield check

	// 	if (shouldApplyDamage && health.Value > 0)
	// 	{
	// 		health.Value -= amount;
	// 		if (health.Value <= 0)
	// 		{
	// 			OnDeath(player);
	// 		}
	// 	}
	// }
