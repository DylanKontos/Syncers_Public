using System.Collections;
using FishNet.Managing.Timing;
using FishNet.Object;
using UnityEngine;

public class BotWeapons : NetworkBehaviour
{
    public Camera botRightWeaponCamera;
    private Transform botRightWeaponCameraTransform;

    public Camera botLeftWeaponCamera;
    private Transform botLeftWeaponCameraTransform;

    public Camera botLaserCamera;
    private Transform botLaserCameraTransform;

    // TestPredictedProjectile
    public Transform firePointRight;
    public Transform firePointLeft;

    public NetworkObject projectilePrefabLeft;
    public NetworkObject projectilePrefabRight;

    public TrailRenderer raycastFrontShootTrail; // Trail of raycast
    public LayerMask controlledShipLayer; // What we can hit with our raycast bullets

    // RAYCAST DAMAGE // RAYCAST DAMAGE // RAYCAST DAMAGE 
    private int raycastFrontDamage = 10;

    // Server Cooldown Timers Font Laser
    private float nextFireTimeRaycast = 0f; 
    private float fireRateRaycast = 0.25f; // 0.25 in FPSland
    
    // Weapons/Cannon fire rates
    private float fireRateWeapon = 1f; // 0.25 in FPSland
    // Server Cooldown Timers Right Weapon
    private float nextFireTimeRightWeapon = 0f; 
    // Server Cooldown Timers Left Weapon
    private float nextFireTimeLeftWeapon = 0f; 

    
    public override void OnStartNetwork()
    {
        TimeManager.OnTick += TimeManager_OnTick; // Used for predicted bullet
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

        botRightWeaponCameraTransform = botRightWeaponCamera.transform;    
        botLeftWeaponCameraTransform = botLeftWeaponCamera.transform;    
        botLaserCameraTransform = botLaserCamera.transform;    
    }

    private void TimeManager_OnTick()
    {
        if (this == null) return;

        BotCheckLaserRaycast();
        BotCheckRightWeaponRaycast();
        BotCheckLeftWeaponRaycast();
    }

    #region check raycasts
    #endregion check raycasts

    private void BotCheckLaserRaycast()
    {
        if (!Physics.Raycast(botLaserCameraTransform.position, botLaserCameraTransform.forward, out RaycastHit hit, Mathf.Infinity, controlledShipLayer)) return;
        if (!BotLaserFireTimeMetRaycast()) { return; }

        BotFireRaycastForward(default, botLaserCameraTransform.position, botLaserCameraTransform.position);
    }

    private void BotCheckRightWeaponRaycast()
    {
        if(!Physics.Raycast(botRightWeaponCameraTransform.position, botRightWeaponCameraTransform.forward, out RaycastHit hit, Mathf.Infinity, controlledShipLayer)) return;
        if (!BotRightWeaponFireTimeMet()) { return; }

        PoolSpawnRight();
    }

    private void BotCheckLeftWeaponRaycast()
    {
        if(!Physics.Raycast(botLeftWeaponCameraTransform.position, botLeftWeaponCameraTransform.forward, out RaycastHit hit, Mathf.Infinity, controlledShipLayer)) return;
        if (!BotLeftWeaponFireTimeMet()) { return; }

        PoolSpawnLeft();
    }

    #region fireRaycast
    #endregion fireRacyast

    private void BotFireRaycastForward(PreciseTick pt, Vector3 position, Vector3 forward)
    {
        nextFireTimeRaycast = Time.time + fireRateRaycast; 

        if(!Physics.Raycast(botLaserCameraTransform.position, botLaserCameraTransform.forward, out RaycastHit hit)) return;

        if (base.IsServerInitialized) // THE CLIENT WILL NOT RUN THIS! 
        {
            ServerSpawnTrail(botLaserCameraTransform.position, hit.point); // Let the server take care of trails
            // Observers at the end of this chain will ensure all clients see trails...

            if(hit.transform.TryGetComponent(out ControlledShip hitControlledShip))
            {
                // Debug.Log("Attempting to call ReceiveDamage on Hit controlledShip");
                // if (hitControlledShip == controlledShip ) return; // prevent self killing
                // Player playerLocal = controlledShip.player.Value; // THIS PLAYER // AKA KILLER PLAYER 
                // // 070325 this is a bot and no player.. so null
                hitControlledShip.ReceiveDamage(raycastFrontDamage, null);
            }

            if(hit.transform.TryGetComponent(out Bot hitBot))
            {
                // Debug.Log("Attempting to call ReceiveDamage on Hit controlledShip");
                // if (hitBot == controlledShip ) return; // prevent self killing
                // Player playerLocal = controlledShip.player.Value; // THIS PLAYER // AKA KILLER PLAYER

                hitBot.ReceiveDamage(raycastFrontDamage, null);
            }

            else 
            {
                // Debug.Log("ControlledShipNotHit..."); 
            }
        }
    }

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
        // Debug.Log("SpawnTrailCalled");
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

    #region fireWeapon
    #endregion fireWeapon

    private void PoolSpawnRight()
    {
        if (!IsServerInitialized) return;

        nextFireTimeRightWeapon = Time.time + fireRateWeapon; 

        NetworkObject nob = NetworkManager.GetPooledInstantiated(projectilePrefabRight, firePointRight.position, firePointRight.rotation, IsServerStarted);
        ServerManager.Spawn(nob, Owner);
        TestProjectile projectile = nob.GetComponent<TestProjectile>();
        projectile.InitializeBot();
    }


    private void PoolSpawnLeft()
    {
        if (!IsServerInitialized) return;

        nextFireTimeLeftWeapon = Time.time + fireRateWeapon; 

        NetworkObject nob = NetworkManager.GetPooledInstantiated(projectilePrefabLeft, firePointLeft.position, firePointLeft.rotation, IsServerStarted);
        ServerManager.Spawn(nob, Owner);
        TestProjectile projectile = nob.GetComponent<TestProjectile>();
        projectile.InitializeBot();
    }

    #region cooldowns
    #endregion cooldowns

    private bool BotLaserFireTimeMetRaycast()
    {
        // Remove allowance calculation
        return (Time.time >= nextFireTimeRaycast);
    }

    private bool BotRightWeaponFireTimeMet()
    {
        // Remove allowance calculation
        return (Time.time >= nextFireTimeRightWeapon);
    }

    private bool BotLeftWeaponFireTimeMet()
    {
        // Remove allowance calculation
        return (Time.time >= nextFireTimeLeftWeapon);
    }
}
