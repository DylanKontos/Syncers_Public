using System.Collections;
using FishNet.Object;
using UnityEngine;
using static Player;
public class PredictedProjectile : MonoBehaviour    // This class is on the spawned projectile // ControlledShip will read this
{
    public Player ownerPlayer { get; private set; } // Player who fired the projectile, passed in from PlayerProjectileSystem   // NOT CONTROLLED SHIP
    
    public int damage { get; private set; } = 100;     // public int damage = 50; 
    private int lifetime = 4; // Lifetime in seconds

    public Material blueProjectileMaterial;
    public Material redProjectileMaterial;
    public Renderer projectileRenderer;
    public Collider projectileCollider;
    public Rigidbody rb;

    private void Awake()
    {
        projectileRenderer = GetComponent<Renderer>();
    }

    public void Initialize(Player player)
    {
        DestroyAfterTimeout();  // StartCoroutine(DestroyAfterTimeout()); // self-destruct if alive for x seconds
        
        ownerPlayer = player; // Store the owner player  // So long as this is passed in, ControlledShip will access this, and run logic based on it.

        if (ownerPlayer.playerTeam.Value == Team.Blue){projectileRenderer.material = blueProjectileMaterial;}
        if (ownerPlayer.playerTeam.Value == Team.Red){projectileRenderer.material = redProjectileMaterial;}
    }

    private void Update()
    {
        // Debug.Log(ownerPlayer.controlledShip.Value.transform.position);
        // if (!IsServerInitialized) return;
        // Debug.Log(ownerPlayer);
        // transform.position = ownerPlayer.controlledShip.Value.transform.position;
        // Debug.Log(projectileCollider);
        // Move();  // To be called with lag-compensated projectile
    }

    private void OnCollisionEnter(Collision collision)  
    {
        Debug.Log("OnCollisionEnter");
        // PlayerObjectWeapons playerObjectWeapons =  ownerPlayer.GetComponent<PlayerObjectWeapons>();
        // playerObjectWeapons._spawnedBullets.Add(collision.gameObject);
        // Debug.Log("PredictedProjectile: OnCollisionEnter, collision.collider: " + collision.collider + "ownerPlayer: " + ownerPlayer);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) // Projectiles instantaited locally, Sometimes projectile doesn't allign 100%
    {
        // Debug.Log("PredictedProjectile: OnTriggerEnter, collision.collider: " + other + " ownerPlayer: " + ownerPlayer );
        Destroy(gameObject);
    }

    private IEnumerator DestroyAfterTimeout()  // In Unity, coroutines are automatically stopped if the object is destroyed
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
        StopAllCoroutines();
    }
}