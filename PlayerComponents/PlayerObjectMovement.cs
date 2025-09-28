using FishNet.Object; 
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public sealed class PlayerObjectMovement : NetworkBehaviour
{
    public bool isAddingForce { get; private set; } = false;
    public bool isBoosting { get; private set; } = false;
    private bool isRotatingRight = false;
    private bool isRotatingLeft = false;
    private bool isThrusterActive = false;
    private bool isBoostThrusterActive = false;

    [SerializeField]
    public int forwardForce = 300; // 300;  // Serialized  - Change in inspector 1st.

    [SerializeField]
    public int torqueForce = 100; // 100;  // Serialized  - Change in inspector 1st.

    // Box Collider - Is Trigger 
    // Mass 2.5 &&  Drag/AngDrag = 5
    // forwardForce = 400;
    // torqueForce = 15;

    public ParticleSystem topEngineThrusterFX;
    public ParticleSystem bottomEngineThrusterFX;

    public ParticleSystem freighterTopEngineThrusterFX;
    public ParticleSystem freighterBottomEngineThrusterFX;

    public ParticleSystem strikerTopEngineThrusterFX;
    public ParticleSystem strikerBottomEngineThrusterFX;

    public ParticleSystem hunterTopEngineThrusterFX;
    public ParticleSystem hunterBottomEngineThrusterFX;

    public ParticleSystem spectreTopEngineThrusterFX;
    public ParticleSystem spectreBottomEngineThrusterFX;

    // public ParticleSystem shieldFX;

    // public ParticleSystem playerWarpDriveFX;

    private PredictionRigidbody rb;
    PlayerObjectInput playerObjectInput; 
    PlayerObjectCameraController playerObjectCameraController;
    ControlledShip controlledShip;

    public struct ReplicateData : IReplicateData // Stores client inputs
    {  
        public bool IsForwardPerformed; // Creating a bool variable to be set in MoveData BuildMoveData() // 
        public bool IsRightPerformed;
        public bool IsLeftPerformed;
        public bool IsBoosting;

        public float Horizontal;
        public float Vertical;

        public ReplicateData(float horizontal, float vertical, bool isForwardPerformed, bool isRightPerformed, bool isLeftPerformed, bool isBoosting, bool isAddingForce)  : this()
        {
            IsForwardPerformed = isForwardPerformed;  
            IsRightPerformed = isRightPerformed;
            IsLeftPerformed = isLeftPerformed;
            IsBoosting = isBoosting;

            Horizontal = horizontal;
            Vertical = vertical;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    // anything that affects transform and how it moves
    public struct ReconcileData : IReconcileData  // how to reset object to server value. Server sets the values and then sends to client
    {
        public readonly PredictionRigidbody Rb;
        public ReconcileData(PredictionRigidbody _rb) : this()
        {
            Rb = _rb;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

	public override void OnStartNetwork()
	{
		base.OnStartNetwork();

        rb = GetComponent<PhysicsObject>().rb;
        // Input and Player //
        playerObjectInput = GetComponent<PlayerObjectInput>();
        playerObjectCameraController = FindObjectOfType<PlayerObjectCameraController>();
        controlledShip = GetComponent<ControlledShip>();

        base.TimeManager.OnTick += TimeManager_OnTick; // just before physics simulation           // Care for respawns?
        base.TimeManager.OnPostTick += TimeManager_OnPostTick; // right after phsysics simulation   // Unsubscribe/Subscribe needed?

        //playerInput.actions.FindActionMap("FreeLookCameraControls").Disable();   !!!BROKEN FOR BUILDS!!
        // player = FindObjectOfType<Player>(); // WILL NOT find the correct player. See GetScoreboardButtonState() in Player
	}

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        base.TimeManager.OnTick -= TimeManager_OnTick;  
        base.TimeManager.OnPostTick -= TimeManager_OnPostTick;
    }

    public void TimeManager_OnTick() // Called right before a physics simulation
    {
        Move(MoveInput());  // Replicate
    }

    private ReplicateData MoveInput() // Build the move data
    {
        if (!base.IsOwner) return default;
          
        bool isForwardPerformed = playerObjectInput.inputStates.ForwardButton;  // Set the bool from PlayerObjectInput
        bool isRightPerformed = playerObjectInput.inputStates.RightButton;
        bool isLeftPerformed = playerObjectInput.inputStates.LeftButton;
        bool isBoosting = playerObjectInput.inputStates.BoostButton;
        bool isAddingForce = false;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        ReplicateData md = new ReplicateData(horizontal, vertical, isForwardPerformed, isRightPerformed, isLeftPerformed, isBoosting, isAddingForce);

        return md;
    }

    [Replicate]
    private void Move(ReplicateData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {


 
        // Vector3 forces = new Vector3(md.Horizontal, 0f, md.Vertical) * 150;
        // rb.AddForce(forces);




        if (state.IsCreated())
        {
            // Forward 
            if (md.IsForwardPerformed == true) // && isAddingForce == false)
            {
                rb.AddForce(transform.forward * forwardForce);
                if (isAddingForce == false) { isAddingForce = true; } // Getting set to true every tick
            }
            
            if (md.IsForwardPerformed == false && isAddingForce == true) 
            {  
                isAddingForce = false;  
            }

            // Right
            if (md.IsRightPerformed == true) 
            {
                rb.AddTorque(transform.up * torqueForce);
                if(isRotatingRight == false) { isRotatingRight = true; }
            }

            if (md.IsRightPerformed == false && isRotatingRight == true)
            {
                isRotatingRight = false; 

            }

            // Left
            if (md.IsLeftPerformed == true) 
            {
                rb.AddTorque(-transform.up * torqueForce);
                if(isRotatingLeft == false) { isRotatingLeft = true; }
            }

            if (md.IsLeftPerformed == false && isRotatingLeft == true)
            {
                isRotatingLeft = false; 

            }

            // Boosting
            if (md.IsBoosting == true && controlledShip.sync.Value >= 1  ) // passed in md.IsBoosting && internal isBoosting
            {
                if (controlledShip.sync.Value <= 0) 
                { 
                    isBoosting = false; 
                    md.IsBoosting = false;

                    return;
                }

                controlledShip.ReduceSync(); // Drains sync
                rb.AddForce(transform.forward * forwardForce);
                if (isBoosting == false) { isBoosting = true; } // Getting set to true every tick
            }
            
            if (md.IsBoosting == false && isBoosting == true) 
            {  
                isBoosting = false;
                isAddingForce = false;    // why here?
            }

            if (md.IsBoosting == false)
            {
                controlledShip.RegenerateSync();
            }
        }

        rb.Simulate();
        
    }

    private void TimeManager_OnPostTick()  // called After physics simulation occurs
    {
        CreateReconcile();
    }
    public override void CreateReconcile()
    {
        ReconcileData rd = new(rb);
        Reconcile(rd);
    }

    [Reconcile]
    private void Reconcile(ReconcileData data, Channel channel = Channel.Unreliable)
    {
        rb.Reconcile(data.Rb);
    }

    private void Update()  
    {
        if (!IsOwner) return;

            float horizontal = 0f;
            float vertical = Input.GetAxisRaw("Vertical");

            if (Input.GetKey(KeyCode.W))
                horizontal += 1f;
            if (Input.GetKey(KeyCode.S))
                horizontal -= 1f;
            if (Input.GetKey(KeyCode.A))
                vertical -= 1f;
            if (Input.GetKey(KeyCode.D))
                vertical += 1f;

        CameraHoldPerformed();
        EngineThrusterPerformed();
        BoostThrusterPerformed();
    }

    private void EngineThrusterPerformed()
    {
        if (isAddingForce && !isThrusterActive)
        {
            EngineThrusterPlay();
            isThrusterActive = true;
        }

        if (!isAddingForce && isThrusterActive)
        {
            EngineThrusterStop();
            isThrusterActive = false;
        }
    }

    private void BoostThrusterPerformed()
    {
        if (isBoosting && !isBoostThrusterActive)
        {
            BoostEngineThrusterPlay();
            isBoostThrusterActive = true;
        }

        if (!isBoosting && isBoostThrusterActive)
        {
            BoostEngineThrusterStop();
            isBoostThrusterActive = false;
        } 
    }

    // non - prediction code // // non - prediction code // // non - prediction code // // non - prediction code // 
    // non - prediction code // // non - prediction code // // non - prediction code // // non - prediction code // 
    // non - prediction code // // non - prediction code // // non - prediction code // // non - prediction code // 

    public void SetPlayerVFXFreighter()
    {
        topEngineThrusterFX = freighterTopEngineThrusterFX;
        bottomEngineThrusterFX = freighterBottomEngineThrusterFX;
    }

    public void SetPlayerVFXStriker()
    {
        topEngineThrusterFX = strikerTopEngineThrusterFX;
        bottomEngineThrusterFX = strikerBottomEngineThrusterFX;
    }

    public void SetPlayerVFXHunter()
    {
        topEngineThrusterFX = hunterTopEngineThrusterFX;
        bottomEngineThrusterFX = hunterBottomEngineThrusterFX;
    }

    public void SetPlayerVFXSpectre()
    {
        topEngineThrusterFX = spectreTopEngineThrusterFX;
        bottomEngineThrusterFX = spectreBottomEngineThrusterFX;
    }


    private void CameraHoldPerformed() // Logic here as PlayerObjectCameraController is on the CAMERA, attempting to reading input broke camera when referencing.
    {
        bool isCameraHoldButtonPressed = playerObjectInput.inputStates.CameraHoldButton;

        if (isCameraHoldButtonPressed == true)
        {
            playerObjectCameraController.ActivateFreeLookCamera();
        }
        
        else 
        {
            playerObjectCameraController.LockCamera();
            // playerObjectCameraController.RecenterCamera();
        }
    }


    // If the server is triggering or calling the beginning of an FX.play, then you just need an ObserversRpc
    // Otherwise, you'll need to Client --> ServerRpc --> ObserversRpc...
    // So for prediction when the client is in charge of movement and FX relating to that. You need this chain..

    // start Shield  // This must be called ON THE CLIENT INITIALLY!!! TargetRPC begins chain...
    // [Client]
    // public void ActivateShieldFX()
    // {
    //     if (!IsOwner) return; 
    //     shieldFX.Play();
    //     // shieldFX.gameObject.SetActive(true);
    //     ActivateShieldFXServerRpc();
    // }

    // [ServerRpc]
    // private void ActivateShieldFXServerRpc()
    // {
    //     shieldFX.Play();
    //     // shieldFX.gameObject.SetActive(true);
    //     ActivateShieldFXObserversRpc();
    // }

    // [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
    // private void ActivateShieldFXObserversRpc()
    // {   
    //     shieldFX.Play();
    //     // shieldFX.gameObject.SetActive(true);
    // }

    // [Client]
    // public void DeactivateShieldFX()
    // {
    //     if (!IsOwner) return; 
    //     shieldFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);    
    //     // shieldFX.gameObject.SetActive(false);
    //     DeactivateShieldFXServerRpc();
    // }

    // [ServerRpc]
    // private void DeactivateShieldFXServerRpc()
    // {
    //     shieldFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);    
    //     DeactivateShieldFXObserversRpc();
    // }

    // [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
    // private void DeactivateShieldFXObserversRpc()
    // {   
    //     shieldFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);    
    //     // shieldFX.gameObject.SetActive(false);
    // }
    // end Shield



    // EngineThrusters // 
    [Client]
    private void EngineThrusterPlay()
    {
        if (!IsOwner) return; 

        topEngineThrusterFX.Play();
        EngineThrusterPlayServerRpc();
    }

    [ServerRpc]
    private void EngineThrusterPlayServerRpc()
    {
        topEngineThrusterFX.Play();
        EngineThrusterPlayObserversRpc();
    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
    private void EngineThrusterPlayObserversRpc()
    {   
       topEngineThrusterFX.Play(); 
    }

    [Client]
    private void EngineThrusterStop()
    {
        if (!IsOwner) return; 

        topEngineThrusterFX.Stop();
        EngineThrusterStopServerRpc();
    }

    [ServerRpc]
    private void EngineThrusterStopServerRpc()
    {
        topEngineThrusterFX.Stop();
        EngineThrusterStopObserversRpc();
    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = true)]
    private void EngineThrusterStopObserversRpc()
    {
       topEngineThrusterFX.Stop(); 
    }

    // Boost Engines //
    [Client]
    private void BoostEngineThrusterPlay()
    {
        if (!IsOwner) return; 

        bottomEngineThrusterFX.Play();
        BoostEngineThrusterPlayerServerRpc();
    }

    [ServerRpc]
    private void BoostEngineThrusterPlayerServerRpc()
    {
        bottomEngineThrusterFX.Play();
        BoostEngineThrusterPlayObserversRpc();
    }

    [ObserversRpc]
    private void BoostEngineThrusterPlayObserversRpc()
    {
        bottomEngineThrusterFX.Play();
    }

    [Client]
    private void BoostEngineThrusterStop()
    {
        if (!IsOwner) return; 

        bottomEngineThrusterFX.Stop();
        BoostEngineThrusterStopServerRpc();
    } 

    [ServerRpc]
    private void BoostEngineThrusterStopServerRpc()
    {
        bottomEngineThrusterFX.Stop();
        BoostEngineThrusterStopObserversRpc();
    }

    [ObserversRpc]
    private void BoostEngineThrusterStopObserversRpc()
    {
        bottomEngineThrusterFX.Stop();
    }

    // private void WarpDrivePlayer()
    // {           // speed?        // TO:DO  Plays when you spam rotate and hold shift
    //     if (rb.velocity.magnitude >= 18f && isRotatingLeft == false && isRotatingRight == false )   // & if (right OR left )
    //     {
    //         playerWarpDriveFX.Play();
    //     }
        
    //     else
    //     {
    //         playerWarpDriveFX.Stop(); 
    //     }
    // }

}


