using System.Runtime.Serialization.Formatters;
using FishNet.Component.Prediction;
using FishNet.Object;
using UnityEngine;
using UnityEngine.AI;
using Syncers.Interfaces; // Import the namespace where IDamageable is defined

public class Bot : NetworkBehaviour, IDamageable
{
    private Rigidbody rb;

    public BoxCollider boxCollider;      // !! ALERT !! Made public for player to call in respawn
    private Vector3 targetPosition;
    private BotManager botManager;
    private Transform botManagerTransform;
    public NavMeshAgent agent;

    public HealthBar healthBar;	

    private bool isAlive = false;

    private float nextTargetTime;
    float range = 250;
    float targetTime = 3f;
    private int health = 100;
    public float explosionFXDuration = 3f; // Duration for which the explosion effect plays

    public ParticleSystem botSpectreTopEngineThrusterFX;
	public ParticleSystem botHitExplosion;
    public GameObject botDeathExplosionFXPrefab; // Instantiated as GO as bot despawns on death...
    private NetworkCollision _networkCollision;


    void Start()
    {
        if (!IsServerInitialized) return;

        healthBar = GetComponentInChildren<HealthBar>();
		if (healthBar != null ) { 		healthBar.ResetHealthBar(); }


        // if (asteroid) {speed = 15}
        GameObject navMeshObject = GameObject.Find("NavMeshSurfaceAsteroid") ?? GameObject.Find("NavMeshSurfaceMonument");

        if (navMeshObject.name == "NavMeshSurfaceAsteroid")
        {
            range = 150;
            agent.speed = 15;
        }
        
        botManager = FindFirstObjectByType<BotManager>();
        botManagerTransform = botManager.GetComponent<Transform>();

        SetRandomTargetPosition();
        nextTargetTime = Time.time + targetTime; // Set the initial time for the next target position update
    }

    public void Awake()
	{
        // _networkCollision = boxCollider.GetComponent<NetworkCollision>();
		// _networkCollision.OnEnter += NetworkCollisionEnter; // Subscribe to the desired collision event
		// RestoreHealth();
	}

    private void OnEnable()
    {
        if (!IsServerInitialized) return;

        isAlive = true; // OnEnable(); allows for future pooling of bots/npcs.

    }

    void Update()
    {
        if (!IsServerInitialized) return;

        if (Time.time >= nextTargetTime)
        {
            SetRandomTargetPosition();
            nextTargetTime = Time.time + targetTime; // Update the next target time
        }

        MoveToTarget();
        
        ProcessEngineVFX();
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

    public void ReceiveDamage(int amount, Player killerPlayer) // pass in damage and player who created projectile that hit this ship.
	{
		if (!IsServerInitialized) return; // If the server is not active, return;

		if (health > 0) 
		{
            PlayBotHitExplosion();
			health -= amount;


            if (healthBar != null)
            {
                healthBar.TakeDamage(amount);
            }

            if (health <= 0)
            {
                // Must be killer player
                BotOnDeath();
                // PASSING IN KILLER PLAYER CAUSES THE BOT TO FAIL ONDEATH();
                // BotOnDeath(killerPlayer != null ? killerPlayer : null); // Passing in the killer player if not null
            }
		}

        else
        {
            BotOnDeath();
            // PASSING IN KILLER PLAYER CAUSES THE BOT TO FAIL ONDEATH();
            // BotOnDeath(killerPlayer != null ? killerPlayer : null); // Passing in the killer player if not null
        }
	}

    #region DEATH
    #endregion DEATH


	// private void BotOnDeath(Player killerPlayer)     

	private void BotOnDeath()     
	{
		if (!IsServerInitialized) return; // SERVER CHECK

        // isAlive = false;

        if (botDeathExplosionFXPrefab != null)
        {
            GameObject explosionFX = Instantiate(botDeathExplosionFXPrefab, transform.position, Quaternion.identity);
            Destroy(explosionFX, explosionFXDuration); // Destroy the explosion effect after the specified duration
            PlayBotDeathExplosionObserversRpc();
        }

        // if (killerPlayer != null) 
		// { 
		// 	killerPlayer.AddKill();
		// }

        // This may be null....
        NetworkObject botToDespawn;
        botToDespawn = GetComponent<NetworkObject>();

        if (botToDespawn != null )
        {
            botManager.DespawnBot(botToDespawn); // Despawn this specific bot
        }

        if (botToDespawn == null)
        {
            botManager.DespawnAllBots();
        }
	}

    [ObserversRpc (ExcludeOwner = true)]
    private void PlayBotDeathExplosionObserversRpc()
    {
        GameObject explosionFX = Instantiate(botDeathExplosionFXPrefab, transform.position, Quaternion.identity);
        Destroy(explosionFX, explosionFXDuration); // Destroy the explosion effect after the specified duration
    }

    [ObserversRpc(ExcludeOwner = false)]   
	private void PlayBotHitExplosion()
	{
		botHitExplosion.Play();
	}

    private void ProcessEngineVFX()
    {
        if (agent.velocity.magnitude > 0)
        {
            if (!botSpectreTopEngineThrusterFX.isPlaying)
            {
                EngineThrusterPlayObserversRpc();
            }
        }

        else
        {
            if (botSpectreTopEngineThrusterFX.isPlaying)
            {
                EngineThrusterStopObserversRpc();
            }
        }
    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
    private void EngineThrusterPlayObserversRpc()
    {   
       botSpectreTopEngineThrusterFX.Play(); 
    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
    private void EngineThrusterStopObserversRpc()
    {
       botSpectreTopEngineThrusterFX.Stop(); 
    }


    void MoveToTarget()
    {
        if (!IsServerInitialized) return;
        // Set the NavMeshAgent destination to the target position
        agent.SetDestination(targetPosition);
    }


    void SetRandomTargetPosition()
    {
        // Define the range for random points
        if (!IsServerInitialized) return;

        // Try to find a valid random position on the NavMesh within the specified range
        Vector3 randomPoint = botManagerTransform.position + Random.insideUnitSphere * range;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
        {
            targetPosition = hit.position;
        }
        else
        {
            Debug.Log("Failed to find a valid target position on the NavMesh.");
        }
    }
}
