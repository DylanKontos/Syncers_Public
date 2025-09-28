using UnityEngine;
using FishNet.Object;
using System.Collections;
using FishNet.Managing.Timing;
using FishNet;

public sealed class PlayerObjectWeapons : NetworkBehaviour
{

    ControlledShip controlledShip; // corecctly finding controlledShip specific to player. Checked via setting public and checking in inspector.
    private Player localPlayer;      // Trying to store the shooter player // causes bugs to late joiners as its a SyncVar fetch...
    private int shooterObjectId;
    PlayerObjectInput playerObjectInput;
    PlayerObjectMovement playerObjectMovement;

    public GameObject leftLaserPointerPrefab;
    public GameObject rightLaserPointerPrefab;
    public Camera laserCamera;
    private Transform laserCameraTransform;

    // bool isOnePerformed = false;

    // PROJECTILE SIDE SHOOT
    private bool isShootingRight = false;
    private bool isShootingLeft = false;

    // Local Client Cooldown Timers
    private float projectileRightCooldown = 0.4f;
    private float projectileLeftCooldown = 0.4f;

    // Server Cooldown Timers
    private float nextFireTimeProjectileRight = 0f; 
    private float nextFireTimeProjectileLeft = 0f; 
    private float fireRateProjectile = 0.5f; // Client 3f

    // RAYCAST FRONT SHOOT 
    private int raycastFrontDamage = 10;
    private bool isShootingSpacebar = false;

    // Local Client Cooldown Timers
    private float raycastLocalCooldown = 0.35f;
    private float lastSpacebarPressTime = 0f;

    // Server Cooldown Timers
    private float nextFireTimeRaycast = 0f; 
    private float fireRateRaycast = 0.25f; // 0.25 in FPSland


    // public ParticleSystem raycastFrontShootTrailFX; // Not quite as accurate as TrailRenderer, but alot simpler to integrate. ArgonLaserFX
	public TrailRenderer raycastFrontShootTrail; // Trail of raycast
    public LayerMask controlledShipLayer; // What we can hit with our raycast bullets

    // public enum FirePoint { None, Left, Right }
    // public FirePoint selectedFirePoint { get; private set; }
    // FirePoint selectedFirepoint; // Internal enum to set on client and then send selectedFirePoint to server

    // Team to send in initialize, sending SyncVar over a ClientPredictedObject causes bugs...
    private bool isRedTeam;

    // TestPredictedProjectile
    public Transform firePointRight;
    public Transform firePointLeft;

    // Right
    public NetworkObject projectilePrefabRight;
    private bool shootQueuedRight;
    private bool useRightFirePoint; // Used as a pass in for a default projectilePrefab but movement pass in is difficult OnSpawnProjectile();

    // Left
    public NetworkObject projectilePrefabLeft;
    private bool shootQueuedLeft;
    private bool useLeftFirePoint; // Used as a pass in for a default projectilePrefab but movement pass in is difficult.


    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
                
        TimeManager.OnTick += TimeManager_OnTick; // Used for predicted bullet
        playerObjectMovement = GetComponent<PlayerObjectMovement>();
        controlledShip = GetComponent<ControlledShip>();
        playerObjectInput = GetComponent<PlayerObjectInput>();

        // Debug.Log(controlledShip.player.Value); // Too late or not being assigned in OnStartNetwork()
    }

    private void Awake()
    {
        laserCameraTransform = laserCamera.transform;    
    }



    #region ONTICK
    #endregion ONTICK


    private void TimeManager_OnTick()
    {
        if (shootQueuedRight)
        {
            if (!FireTimeMetProjectileRight())
            {
                shootQueuedRight = false; // Reset flag if FireTime not met // ADDED 1228PM
                return;
            }
            // nextFireTimeProjectileRight = InstanceFinder.TimeManager.Tick + 60; ???? 
            // Test
            nextFireTimeProjectileRight = Time.time + fireRateProjectile;
            PoolSpawnRight();
            shootQueuedRight = false;
        }

        if (shootQueuedLeft)
        {
            if (!FireTimeMetProjectileLeft())
            {
                shootQueuedLeft = false; // Reset flag if FireTime not met // ADDED 1228PM
                return;
            }
            
            nextFireTimeProjectileLeft = Time.time + fireRateProjectile;
            PoolSpawnLeft();
            shootQueuedLeft = false;
        }
    }

    // [ServerRpc]
    private void PoolSpawnRight()
    {
        // if (!IsOwner) return;
        // OffSet. Was necessary when calling spawn through Server and Observers. ClientPredictedSpawn not needed.
        // if (playerObjectMovement.isAddingForce == true) {firePointRight.position += transform.forward * 3;}
        // if (playerObjectMovement.isBoosting == true) {firePointRight.position += transform.forward * 6 ;}
        // Vector3 position = selectedFirepoint == FirePoint.Right ? firePointRight.position : firePointLeft.position;
        // Quaternion rotation = selectedFirepoint == FirePoint.Right ? firePointRight.rotation : firePointLeft.rotation;

        NetworkObject nob = NetworkManager.GetPooledInstantiated(projectilePrefabRight, firePointRight.position, firePointRight.rotation, IsServerStarted);
        ServerManager.Spawn(nob, Owner);
        CheckTeam();

        // shooterObjectId = OwnerId;
        // localPlayer = controlledShip.player.Value;

        TestProjectile projectile = nob.GetComponent<TestProjectile>();
        projectile.Initialize(isRedTeam);


        // projectile.isPlayerProjectile = true; // May need SyncVar, not necessary... 
        // InitializeServerRpc(projectile, isRedTeam, OwnerId);

    }

    // [ServerRpc]
    private void PoolSpawnLeft()
    {
        NetworkObject nob = NetworkManager.GetPooledInstantiated(projectilePrefabLeft, firePointLeft.position, firePointLeft.rotation, IsServerStarted);
        ServerManager.Spawn(nob, Owner);
        CheckTeam();

        TestProjectile projectile = nob.GetComponent<TestProjectile>();
        projectile.Initialize(isRedTeam);
        // projectile.isPlayerProjectile = true; May need Syncvar, not necessary...
        // InitializeServerRpc(projectile, isRedTeam, OwnerId);
    }

    // [ServerRpc]
    public void InitializeServerRpc(TestProjectile projectile, bool isRedTeam, int OwnerId) // Just for MeshRenderer/Colour
    {
        CheckTeam();

        // projectile.Initialize(isRedTeam, OwnerId);
        // InitializeObserversRpc(projectile, isRedTeam);
    }

    // TODO - DO NOT OBSERVER the local player.

    // [ObserversRpc(BufferLast = true, ExcludeOwner = true)]  // ExcludeOwner true for ClientPredictedSpawnedObject
    // public void InitializeObserversRpc(TestProjectile projectile, bool isRedTeam) // Just for MeshRenderer/Colour
    // {
    //     projectile.InitializeObservers(isRedTeam);
    // }

    private void CheckTeam()
    {
        if (controlledShip.player.Value.playerTeam.Value == Player.Team.Blue)
        {
            isRedTeam = false;
        }

        if (controlledShip.player.Value.playerTeam.Value == Player.Team.Red)
        {
            isRedTeam = true;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        RightCannonPerformed(); // shootQueued = true;
        LeftCannonPerformed();
        OnePerformed();
        LeftAltPerformed();
        LeftControlPerformed();
        SpacebarPerformed();
    }

    #region PROJECTILE

    private bool FireTimeMetProjectileLeft()
    {
        if (base.IsClient)
        {
            return (Time.time >= nextFireTimeProjectileLeft);
        }
        else
        {
            float allowance = Mathf.Min(0.016f, fireRateProjectile * 0.15f);
            return (Time.time + allowance >= nextFireTimeProjectileLeft);
        }
    }

    private bool FireTimeMetProjectileRight()
    {
        if (base.IsClient)
        {
            return (Time.time >= nextFireTimeProjectileRight);
        }
        else
        {
            float allowance = Mathf.Min(0.016f, fireRateProjectile * 0.15f);
            return (Time.time + allowance >= nextFireTimeProjectileRight);
        }
    }

    private void RightCannonPerformed()
    {
        bool isRightCannonPerformed = playerObjectInput.inputStates.RightCannonButton;

        if (isRightCannonPerformed && !isShootingRight)
        {
            shootQueuedRight = true;
            isShootingRight = true;
            // selectedFirepoint = FirePoint.Right; // unused
        }

        if (isShootingRight && Time.time >= projectileRightCooldown)
        {
            isShootingRight = false;
        }
    }

    private void LeftCannonPerformed()
    {
        bool isLeftCannonPerformed = playerObjectInput.inputStates.LeftCannonButton;

        if (isLeftCannonPerformed && !isShootingLeft)
        {
            shootQueuedLeft = true;
            // selectedFirepoint = FirePoint.Left;  // unused
            isShootingLeft = true;
            // projectileLeftCooldown = Time.time + 3.0f;
        }

        if (isShootingLeft && Time.time >= projectileLeftCooldown)
        {
            isShootingLeft = false;
        }
    }

    #endregion PROJECTILE
    #region RAYCAST


    private bool FireTimeMetRaycast()
    {
        if (base.IsClient)
        {
            return (Time.time >= nextFireTimeRaycast);
        }
        else
        {
            float allowance = Mathf.Min(0.016f, fireRateRaycast * 0.15f);
            return (Time.time + allowance >= nextFireTimeRaycast);
        }
    }

    // float timeRequired = 0.1f; // 1 second cooldown between casts.
    // double currentTickTime = base.TimeManager.TicksToTime(new PreciseTick(base.TimeManager.GetPreciseTick(TickType.Tick).Tick, (byte)0));
    // double timePassed = currentTickTime - _lastCastTick;
    // return timePassed >= timeRequired;

    private void SpacebarPerformed()
    {
        bool isSpacebarPerformed = playerObjectInput.inputStates.spacebarButton;

        if (isSpacebarPerformed && !isShootingSpacebar)
        {
            // shootQueuedSpacebar = true; // Use to call in OnTick
            isShootingSpacebar = true;
            lastSpacebarPressTime = Time.time + raycastLocalCooldown;
            
            if (!FireTimeMetRaycast())
            {
                return;
            }

            FireRaycastForward(default, laserCameraTransform.position, laserCameraTransform.position);
            FireRaycastForwardServerRpc(base.TimeManager.GetPreciseTick(TickType.Tick), laserCameraTransform.position, laserCameraTransform.forward);
        }

        if (isShootingSpacebar && Time.time >= lastSpacebarPressTime)
        {
            isShootingSpacebar = false;
        }
    }

    private void FireRaycastForward(PreciseTick pt, Vector3 position, Vector3 forward)
    {

        nextFireTimeRaycast = Time.time + fireRateRaycast; // This is FUCKING UP added local cd's all g

        // _lastCastTick = base.TimeManager.TicksToTime();
        // _lastCastTick = base.TimeManager.TicksToTime(new PreciseTick(base.TimeManager.GetPreciseTick(TickType.Tick).Tick, (byte)0));

        if(!Physics.Raycast(laserCameraTransform.position, laserCameraTransform.forward, out RaycastHit hit)) return;

        // FireRaycastForwardClient(laserCameraTransform.position, hit.point);

        if (base.IsServerInitialized) // THE CLIENT WILL NOT RUN THIS! 
        {
            ServerSpawnTrail(laserCameraTransform.position, hit.point); // Let the server take care of trails
            // Observers at the end of this chain will ensure all clients see trails...

            if(hit.transform.TryGetComponent(out ControlledShip hitControlledShip))
            {
                    if (hitControlledShip == controlledShip ) return; // prevent self killing
                    Player playerLocal = controlledShip.player.Value; // THIS PLAYER // AKA KILLER PLAYER
                    hitControlledShip.ReceiveDamage(raycastFrontDamage, playerLocal);
            }

            if(hit.transform.TryGetComponent(out Bot hitBot))
            {
                    if (hitBot == controlledShip ) return; // prevent self killing

                    Player playerLocal = controlledShip.player.Value; // THIS PLAYER // AKA KILLER PLAYER

                    hitBot.ReceiveDamage(raycastFrontDamage, playerLocal);
            }

            else 
            {
            }
        }
    }

    // Call Fire over the network
    [ServerRpc]
    private void FireRaycastForwardServerRpc(PreciseTick pt, Vector3 position, Vector3 forward)
    {
        // if (!base.IsOwner) //Only fire again on server if not client host/owner.
        // {
            if (!FireTimeMetRaycast())  // FPS LAND bool to check fire rate. TODO Test for correct floats... APPLY TO OTHER FIRING METHODS
            {
                return;
            }

            FireRaycastForward(pt, position, forward);
        // }
    }

    // [Client] // FX for the client that fired... Observers not working in WebGL 
    // private void FireRaycastForwardClient(Vector3 startPosition, Vector3 hitPoint)
    // {
    //     TrailRenderer trail = Instantiate(raycastFrontShootTrail, startPosition, Quaternion.identity);
    //     StartCoroutine(SpawnTrail(trail, startPosition, hitPoint));
    // }

    private void ServerSpawnTrail(Vector3 startPosition, Vector3 hitPoint)
    {
        TrailRenderer trail = Instantiate(raycastFrontShootTrail, startPosition, Quaternion.identity);
        StartCoroutine(SpawnTrail(trail, startPosition, hitPoint));
        ObserversRpcSpawnTrail(startPosition, hitPoint );
    }

    [ObserversRpc(ExcludeOwner = false)]
    private void ObserversRpcSpawnTrail(Vector3 startPosition, Vector3 hitPoint)
    {
        TrailRenderer trail = Instantiate(raycastFrontShootTrail, startPosition, Quaternion.identity);
        StartCoroutine(SpawnTrail(trail, startPosition, hitPoint));
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 startPosition, Vector3 hitPoint)
    {
        float time = 0;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }

        trail.transform.position = hitPoint;
        Destroy(trail.gameObject, trail.time);
    }


    #endregion RAYCAST
    #region other
    #endregion other

    private void OnePerformed()
    {
        bool isOnePerformed = playerObjectInput.inputStates.OneButton;

        if (isOnePerformed == true)
        {
  
        }

        if (isOnePerformed == false)
        {

        }
    }

    private void LeftControlPerformed()
    {
        bool isLeftControlPerformed = playerObjectInput.inputStates.LeftControlButton;

        if (isLeftControlPerformed == true && controlledShip.sync.Value >= 1)
        {
            controlledShip.FreezeSync();
            leftLaserPointerPrefab.SetActive(true);
        }

        if (isLeftControlPerformed == false)
        {
            leftLaserPointerPrefab.SetActive(false);
        }
    }

    private void LeftAltPerformed()
    {
        bool isAltControlPerformed = playerObjectInput.inputStates.LeftAltButton;

        if (isAltControlPerformed == true && controlledShip.sync.Value >= 1)
        {
            controlledShip.FreezeSync();
            rightLaserPointerPrefab.SetActive(true);
        }

        if (isAltControlPerformed == false)
        {
            rightLaserPointerPrefab.SetActive(false);
        }
    }
}