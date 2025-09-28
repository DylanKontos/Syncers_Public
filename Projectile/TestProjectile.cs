using System.Collections;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using static Player;
using FishNet;
using FishNet.Component.Prediction;
using FishNet.Object.Synchronizing;
using Syncers.Interfaces;

[RequireComponent(typeof(PhysicsObject))]
public class TestProjectile : NetworkBehaviour
{
    // Causes bug for BOT...  // ADDED NULL CHECK 2203 in OnStartServer
    // public readonly SyncVar<Player> projectileOwnerPlayer = new(); //  SyncVar<Player>();  // which player is controlling the ship // contains name

    // public Player projectileOwnerPlayer { get; private set; } // Player who fired the projectile, passed in from PlayerProjectileSystem   // NOT CONTROLLED SHIP
    // Player fecthes owner of this projectile with:
    // projectileOwnerPlayer
    // Bot fetches with:
    // Player ownerPlayer = testProjectile.Owner.FirstObject.GetComponent<Player>();  // 08/10

    Player projectileOwnerPlayer;

    public int damage { get; private set; } = 40;     // public int damage = 50; 

    public Material blueProjectileMaterial;
    public Material redProjectileMaterial;
    public Renderer projectileRenderer;
    public Collider projectileCollider;

    // private Team localTeam;
    private bool isRedTeam;
    public bool isPlayerProjectile; // natively false // TODO: DELETE 21/03 Not Passing in correctly...

    // Player player; // Found in OnStartNetwork();
    // PlayerObjectWeapons playerObjectWeapons; // Found in OnStartNetwork

    TestProjectileMovementLeft testProjectileMovementLeft;
    TestProjectileMovementRight testProjectileMovementRight;

    private NetworkCollision _networkCollision;

    public DamageData damageData;



    public override void OnStartNetwork()
    {
        projectileRenderer = GetComponent<Renderer>();
        projectileCollider = GetComponent<SphereCollider>();

        StopAllCoroutines();

        // player = base.Owner.FirstObject.GetComponent<Player>(); // Successfullyfound OnStartNetwork()
        // playerObjectWeapons = player.controlledShip.Value.GetComponent<PlayerObjectWeapons>();
        Invoke("DespawnProjectile", 3f);
        // StartCoroutine(Despawn(3));
    }

    public override void OnStartServer()
    {
        if (Owner.FirstObject != null)
        {
            // Just like passing player into RaycastKill, it occurs on server side.
            // Clients only see numbers of scoreboards, they dont need to know the Player...
            projectileOwnerPlayer = Owner.FirstObject.GetComponent<Player>(); 
            // Debug.Log(projectileOwnerPlayer);
            // projectileOwnerPlayer.Value = Owner.FirstObject.GetComponent<Player>();  // SYNCVAR CAUSING BUGS

        }
        
    }

    public void Initialize(bool isRedTeam) 
    {
        // ownerPlayer = player; // Store the owner player  // So long as this is passed in, ControlledShip will access this, and run logic based on it.
        // ownerPlayer = InstanceFinder.ClientManager.Objects.Spawned[shooterObjectId].GetComponent<Player>();
        // ownerPlayer = base.Owner.FirstObject.GetComponent<Player>();
        // Just finding owner on collission instead...

        if (isRedTeam == false) {projectileRenderer.material = blueProjectileMaterial;}
        if (isRedTeam == true) {projectileRenderer.material = redProjectileMaterial;}


        // InitializeObserversRpc(isRedTeam); // 11/10 !DISABLE ON CLIENTCPREDICTED...
        InitializeServerRpc(isRedTeam); // 11/10 !ENABLE ON CLIENTPREDICTEDSPAWN!
        // if (localTeam == Team.Red) {projectileRenderer.material = redProjectileMaterial;}  // BUG
    }

    [ServerRpc]
    public void InitializeServerRpc(bool isRedTeam) 
    {
        if (isRedTeam == false) {projectileRenderer.material = blueProjectileMaterial;}
        if (isRedTeam == true) {projectileRenderer.material = redProjectileMaterial;}

        InitializeObserversRpc(isRedTeam);

        // if (localTeam == Team.Red) {projectileRenderer.material = redProjectileMaterial;}  // BUG
    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = false)]
    public void InitializeObserversRpc(bool isRedTeam) 
    {
        if (isRedTeam == false) {projectileRenderer.material = blueProjectileMaterial;}
        if (isRedTeam == true) {projectileRenderer.material = redProjectileMaterial;}

        // testProjectileMovementLeft = GetComponent<TestProjectileMovementLeft>();
        // testProjectileMovementRight = GetComponent<TestProjectileMovementRight>();

        // if (testProjectileMovementLeft != null) { testProjectileMovementLeft.Move();}
        // if (testProjectileMovementRight != null) { testProjectileMovementRight.Move();}
        // if (localTeam == Team.Red) {projectileRenderer.material = redProjectileMaterial;}  // BUG
    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = false)]
    public void InitializeBot()
    {
        projectileRenderer.material = blueProjectileMaterial;
    }

    private void OnEnable()
    {
        projectileCollider.enabled = true;
        projectileRenderer.enabled = true;
        _networkCollision = GetComponent<NetworkCollision>();
		_networkCollision.OnEnter += NetworkCollisionEnter; // Subscribe to the desired collision event
    }



    // An interface is a "contract" that defines a rule.
    // Any class that implements this interface must have a method TakeDamage()
    // Without knowing or getting the object, it just allows this projectile to deal damage to the collider other
    // So long as it has this interface.

	#region NetworkCollision
    #endregion NetworkCollision




    private void NetworkCollisionEnter(Collider other)
	{
        // Debug.Log(other);

        projectileRenderer.enabled = false;
        damageData = new DamageData(40, projectileOwnerPlayer);
        // damageData = new DamageData(40, projectileOwnerPlayer.Value); SYNCVAR CAUSING BUGS


        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        
        if (damageable != null)
        {
            damageable.TakeDamage(damageData);
        }

        projectileCollider.enabled = false;

        // DisableProjectile();
    }


        // Physics.IgnoreCollision(projectileCollider, other);

        // ControlledShip controlledShip = other.GetComponent<ControlledShip>();
        // Bot bot = other.GetComponent<Bot>();
        
        // if (controlledShip != null)
        // {
        // }

        // if (bot != null)
        // {
        // }

        // else
        // {
        // }

        // DisableProjectile();

    
    // Collision issues if THIS projectile calls this upon NetworkCollisionEnter
    public void DisableProjectile() // Called by bot on NetworkCollision instead...
    {
        if (!IsServerInitialized) return; 

        // RE ENABLE THIS AFTER TESTING 22/03
        projectileCollider.enabled = false;
        projectileRenderer.enabled = false;
    }


    private void DespawnProjectile()
    {
        // Destroy(gameObject);
        ServerManager.Despawn(gameObject); // Test despawn every FishNet update
        // if (gameObject != IsDeinitializing) { ServerManager.Despawn(gameObject); }
    }

    private IEnumerator Despawn(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        // Destroy(gameObject);
        ServerManager.Despawn(gameObject);
    }

    // private void NetworkCollisionEnter(Collider other) // On Collision?
	// {

    // }
    
    // [ServerRpc]  // Attempt to solve Despawn warning... FishNet Prediction issue.
    // private void DespawnServerRpc()
    // {

    //     Destroy(gameObject);
    //     // ServerManager.Despawn(gameObject);
    // }
}

    // private bool moveProjectileRight = false;
    // private bool moveProjectileLeft = false;

    // public enum FirePoint { Left, Right }
    // private FirePoint currentMoveDirection = FirePoint.None;

    // public enum MoveDirection { None, Left, Right }
    // private MoveDirection currentMoveDirection = MoveDirection.None;
        // if (!IsOwner) return; 



        // Left on server, Right on client. and SERVER MOVES THIS!!!!!!

        // This if statement is only read ONCE and thats it. So it will read instantly. 
        // Unless a client mashes both together? Both shoots? Test?
        // SO we'll see if setting left/right mid projectile changes the direction of a existing projectile

        // if (playerObjectWeapons.selectedFirePoint.Value == PlayerObjectWeapons.FirePoint.Right)
        // {
        //     rb.Velocity(transform.right * 80); // Move right
        // }

        // if (playerObjectWeapons.selectedFirePoint.Value == PlayerObjectWeapons.FirePoint.Left)
        // {
        //     rb.Velocity(transform.right * -80); // Move left
        // }
    // }

    // private void Update()
    // {
    // }


    // private IEnumerator DetermineMoveCoroutine()
    // {
    //     while (true)
    //     {
    //         if (projectileFirePoint != FirePoint.None)
    //         {
    //             if (projectileFirePoint == FirePoint.Left)
    //             {
    //                 rb.Velocity(transform.right * -80);
    //                 // MoveLeft();
    //             }
    //             else if (projectileFirePoint == FirePoint.Right)
    //             {
    //                 rb.Velocity(transform.right * 80);
    //                 // MoveRight();
    //             }

    //             // Reset the direction to None after moving
    //             projectileFirePoint = FirePoint.None;

    //             // Break the coroutine after a move has been called
    //             yield break;
    //         }

    //         // Wait for the next frame
    //         yield return null;
    //     }
    // }