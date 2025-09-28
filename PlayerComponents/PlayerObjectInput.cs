using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object; 


public sealed class PlayerObjectInput : NetworkBehaviour // Monobehaiour? No because InputStates will be used in Recon.
{
    public float zoomCameraInput { get; private set; } =  0.0f;
    public bool invertScroll { get; private set; } = true; 
    
    private bool isCameraScrolling = false;
    bool targetButtonReleased = true; // flag to stop target switch spamming

    public InputStates inputStates; // instance of InputStates struct
    PlayerManager playerManager;
    PlayerObjectCameraController playerObjectCameraController;


    private float zoomSpeed = 10f;
    private float zoomAcceleration = 2.5f;  // reduce jerky movement 
    private float zoomInnerRange = 10;  
    private float zoomOuterRange = 350; 

    private float currentMiddleRigRadius;
    private float newMiddleRigRadius; 

    private int currentControlledShipIndex = 0;
    int controlledShipCycleCounter = 0;


    float forwardThreshold = 0.4f; // serialize when play testing
    float lateralThreshold = 1f; // serialize when play testing

    // [SerializeField]
    // float threshold = 0.3f; // mobile d-pad tresh-hold




    private Transform currentTarget;





    
    public struct InputStates // scruct for all input //  !!!FishNet!!!! Collecting input Update --> will feed into MoveData
    {
        public bool ForwardButton;      // Movement Bools for MoveData
        public bool RightButton;
        public bool LeftButton;
        public bool BoostButton;

        public bool CameraHoldButton;   // Weapons/Spells
        public bool CameraFollowTargetButton;
        public bool RightCannonButton;
        public bool LeftCannonButton;
        public bool OneButton;

        public bool LeftControlButton;
        public bool LeftAltButton;

        public bool SpawnButton;
        public bool DisplayScoreboardButton;

        public bool CameraZooming;

        public bool zButton;
        public bool nineButton;
        public bool spacebarButton;

        public bool moveMobile; // Replace WASD on PC
        // public bool lookMobile; // mapped to BUTTON mouse1 press, THEN, Joystick for movement.
        // shoot right, shoot left, & boost, share inputs with  E, Q, LeftShift respectively.
    }

    public override void OnStartNetwork()
	{
		base.OnStartNetwork();
        playerObjectCameraController = FindObjectOfType<PlayerObjectCameraController>();
        playerManager = FindObjectOfType<PlayerManager>();
    }

    public void Start() // Used to initialize the camera zoom. Otherwise the zoom SNAPS to 10 and then slowly zooms out.
    {
        currentMiddleRigRadius = playerObjectCameraController.vc.m_Orbits[1].m_Radius;
        newMiddleRigRadius = currentMiddleRigRadius;
    }

    public void LeftControl(InputAction.CallbackContext context)  
    {
        inputStates.LeftControlButton = context.performed;
    }

    public void LeftAlt(InputAction.CallbackContext context)  
    {
        inputStates.LeftAltButton = context.performed;
    }

    public void One(InputAction.CallbackContext context)  
    {
        inputStates.OneButton = context.performed;
    }

    public void Forward(InputAction.CallbackContext context)   // I could call 'Forward' W, but players can change bindings. Maybe dispalyed as Move Foward in player binding map
    {
        inputStates.ForwardButton = context.performed;  // Setting a bool based on context performed
    }

    public void Right(InputAction.CallbackContext context)
    {
        inputStates.RightButton = context.performed; 
    }

    public void Left(InputAction.CallbackContext context)
    {
        inputStates.LeftButton = context.performed; 
    }

    public void Boost(InputAction.CallbackContext context)
    {
        inputStates.BoostButton = context.performed; 
    } 

    public void RightCannon(InputAction.CallbackContext context)     
    {
        inputStates.RightCannonButton = context.performed; 
    }

    public void LeftCannon(InputAction.CallbackContext context)     
    {
        inputStates.LeftCannonButton = context.performed; 
    }

    public void DisplayScoreboard(InputAction.CallbackContext context)     
    {
        inputStates.DisplayScoreboardButton = context.performed; 
    }

    public void CameraFollowTarget(InputAction.CallbackContext context)  
    {
        inputStates.CameraFollowTargetButton = context.performed;
    }

    public void zButton(InputAction.CallbackContext context)  
    {
        inputStates.zButton = context.performed;
    }

    public void nineButton(InputAction.CallbackContext context)  
    {
        inputStates.nineButton = context.performed;
    }

    public void SpacebarButton(InputAction.CallbackContext context)  
    {
        inputStates.spacebarButton = context.performed;
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Mobile  // Joystick // Buttons are mapped straight to keys /////////////////////////////////////////////////////////
    // The only seperate mobile logic is the left joy-stick mapped to 'moveMoble' //////////////////////////////////////////////  
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


public void moveMobile(InputAction.CallbackContext context)
{
    if (!context.performed)
    {
        StopMoving();
        return;
    }

    Vector2 inputVector = context.ReadValue<Vector2>();

    // Check for forward movement with higher priority             // OR statement allows for downward forward
    inputStates.ForwardButton = inputVector.y > forwardThreshold || inputVector.y < -forwardThreshold;


    // If within the forward threshold range, don't move left or right
    if (inputVector.y > lateralThreshold)
    {
        inputStates.LeftButton = false;
        inputStates.RightButton = false;
    }
    else
    {
        inputStates.LeftButton = inputVector.x < -0.5f;
        inputStates.RightButton = inputVector.x > 0.5f;
    }
}

private void StopMoving()
{
    inputStates.ForwardButton = false;
    inputStates.LeftButton = false;
    inputStates.RightButton = false;
}

    
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Alot of camera logic is here and in PlayerObjectCameraController - unable to isolate logic to PlayerObjectCameraController....
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private void LateUpdate()
    {
        UpdateZoomLevel();
        CameraFollowTargetLogic();
        // CheckLockCameraNull();  // THIS WORKS BUT NEEDS TO BE FLESHED OUT 16/03
    }

    // only works on bots as it checks currentTarget. Players never go null unless d/c.
    // for players to wokr you need to check mesh renderer or isAlive...
    // But it will stay locked onto a player target after a player dies... Thats good. Track target to respawn...
    private void CheckLockCameraNull()
    {
        if (currentTarget == null) // Check if the current target is null
        {
            playerObjectCameraController.TurnOffTargetCameraMode(); // Exit the target camera mode
            controlledShipCycleCounter = 0; // Reset the counter

            // Assuming controlledShip.SetFreeLookCamNameDisplayer() is appropriate here
            if (playerManager.AllControlledShips.Count > 0)
            {
                ControlledShip controlledShip = playerManager.AllControlledShips[0].GetComponent<ControlledShip>();
                if (controlledShip != null)
                {
                    controlledShip.SetFreeLookCamNameDisplayer();
                }
            }
        }
    }

    public Transform CameraFollowTargetLogic() // Finds player transform from PlayerManager
    {
        bool isCameraFollowTargetPerformed = inputStates.CameraFollowTargetButton; // check input bool

        

        if (isCameraFollowTargetPerformed && targetButtonReleased) // && AllControlledShips.Count > 0)
        {
            // YOU MUST PRESS CAPS LOCK TO RUN THIS LOGIC AGAIN
            targetButtonReleased = false; // Set the flag to false as soon as the button is pressed

            currentControlledShipIndex = (currentControlledShipIndex + 1) % playerManager.AllControlledShips.Count; // Cycle to the next ship.
            NetworkObject networkObject = playerManager.AllControlledShips[currentControlledShipIndex]; // Get the current ship from the AllControlledShips list

            ControlledShip controlledShip = networkObject.GetComponent<ControlledShip>();
            Bot bot = networkObject.GetComponent<Bot>();


            // PLAYER //
            // PLAYER //
            // PLAYER //
            if ((controlledShip != null && controlledShip.transform == playerObjectCameraController.playerObject) ||
                (bot != null && bot.transform == playerObjectCameraController.playerObject)) // Prevent passed in controlledShip or bot from being == obj or playerObject (player cannot be target)
            {
                currentControlledShipIndex = (currentControlledShipIndex + 1) % playerManager.AllControlledShips.Count;
                networkObject = playerManager.AllControlledShips[currentControlledShipIndex];
                controlledShip = networkObject.GetComponent<ControlledShip>();
                bot = networkObject.GetComponent<Bot>();
            }

            if (controlledShip != null) // null check for ControlledShip
            {
                Transform controlledShipTransform = controlledShip.transform;
                playerObjectCameraController.SetCameraTarget(controlledShipTransform); // pass through obj vcTarget.LookAt = target
                controlledShip.SetTargetCamNameDisplayer();
                controlledShipCycleCounter++;

                // Store target and CHECK IF NULL WILL NOT WORK FOR PLAYER 
                currentTarget = controlledShipTransform; // Store the target

                if (controlledShipCycleCounter >= playerManager.AllControlledShips.Count) // If you've cycled through all ships
                {
                    playerObjectCameraController.TurnOffTargetCameraMode(); // Exit the target camera mode
                    controlledShipCycleCounter = 0; // Reset the counter
                    controlledShip.SetFreeLookCamNameDisplayer();
                }

                if (controlledShipTransform == null)
                {
                    playerObjectCameraController.TurnOffTargetCameraMode(); // Exit the target camera mode
                    controlledShipCycleCounter = 0; // Reset the counter
                    controlledShip.SetFreeLookCamNameDisplayer(); 
                }

                return controlledShipTransform;
            }


            // BOT //
            // BOT //
            // BOT //

            if (bot.isActiveAndEnabled == false)
            {
                // Debug.Log("bot.isActiveAndEnabled == false");
                playerObjectCameraController.TurnOffTargetCameraMode(); // Exit the target camera mode
                controlledShipCycleCounter = 0; // Reset the counter
                controlledShip.SetFreeLookCamNameDisplayer(); 
            }

            if (bot != null) // null check for Bot
            {
                Transform botTransform = bot.transform;
                playerObjectCameraController.SetCameraTarget(botTransform); // pass through obj vcTarget.LookAt = target
                controlledShipCycleCounter++;
                currentTarget = botTransform; // Store the target

                if (controlledShipCycleCounter >= playerManager.AllControlledShips.Count) // If you've cycled through all ships
                {
                    playerObjectCameraController.TurnOffTargetCameraMode(); // Exit the target camera mode
                    controlledShipCycleCounter = 0; // Reset the counter

                    // controlledShip.SetFreeLookCamNameDisplayer(); 
                    // Assume Bot has SetTargetCamNameDisplayer() method
                    // bot.SetTargetCamNameDisplayer();
                    // Was there another spot for this SetTargetCamName?????
                }

                return botTransform;
            }

        }



        else if (!isCameraFollowTargetPerformed)
        {
            targetButtonReleased = true; // Set the flag to true when button is released
        }

        return null; // Return null if no suitable transform was found.
    }



    ///////////////////////////////////////////////////////////////////////////////
    // Finding controlledShip based on player pass in from AllPlayers list instead, caused issues if there was an observer.
    // The fix would have been a player.team check, but I made a controlledShip list in PlayerManager. More direct. NOW add a team check.
    ///////////////////////////////////////////////////////////////////////////////

    // // Potentially use box collider detector instead. Idk... 
    // public Transform CameraFollowTargetLogic()   // Finds player transform from PlayerManager
    // {
    //     bool isCameraFollowTargetPerformed = inputStates.CameraFollowTargetButton; // check input bool

    //     if (isCameraFollowTargetPerformed && targetButtonReleased ) // && playerManager.AllPlayers.Count > 0)
    //     {
    //         targetButtonReleased = false; // Set the flag to false as soon as button is pressed

    //         currentPlayerIndex = (currentPlayerIndex + 1) % playerManager.AllPlayers.Count;  // Cycle to the next player.
    //         NetworkObject networkObject = playerManager.AllPlayers[currentPlayerIndex];   // Get the current player from the AllPlayers list
    //         Player player = networkObject.GetComponent<Player>();



    //         if (player.controlledShip.transform == playerObjectCameraController.playerObject) // Prevent obj or playerObject from being target
    //         {
    //             currentPlayerIndex = (currentPlayerIndex + 1) % playerManager.AllPlayers.Count;
    //             networkObject = playerManager.AllPlayers[currentPlayerIndex];
    //             player = networkObject.GetComponent<Player>();
    //         }

    //         if (player != null && player.controlledShip != null) // null check
    //         {
    //             Transform controlledShipTransform = player.controlledShip.transform;
    //             playerObjectCameraController.SetCameraTarget(controlledShipTransform); // pass through obj vcTarget.LookAt = target
    //             playerCycleCounter++;

    //             if (playerCycleCounter >= playerManager.AllPlayers.Count) // If you've cycled through all players
    //             {
    //                 playerObjectCameraController.TurnOffTargetCameraMode(); // Exit the target camera mode
    //                 playerCycleCounter = 0; // Reset the counter
    //             }
                
    //             return controlledShipTransform;
    //         }
    //     }

    //     else if (!isCameraFollowTargetPerformed)
    //     {
    //         targetButtonReleased = true; // Set the flag to true when button is released
    //     }

    //     return null;    // Return null if no suitable transform was found.
    // }


    public void CameraHold(InputAction.CallbackContext context)
    {
        inputStates.CameraHoldButton = context.performed;
        
        // Debug.Log(context.performed);
        // playerObjectCameraController.TurnOffTargetCameraMode();
    }

 
    public void CameraZoom(InputAction.CallbackContext context)     // https://www.youtube.com/watch?v=bVoJ3-BMNi0&t=126s
    {        
        float zoomInput = context.ReadValue<float>();   // -120, 120  in unity inputmanager
        float clampedZoomInput = Mathf.Clamp(zoomInput, -100f, 100f);       // clamp here for lower values

        if(context.performed == true) 
        {
            // Debug.Log(zoomInput);
            // Debug.Log(clampedZoomInput);
            // Method that passes in clampedZoomInput

            // Debug.Log(clampedZoomInput); // either 100 or -100

            CameraZoomSetter(clampedZoomInput);

            isCameraScrolling = true; 
        }
        
        if(context.performed == false) 
        {
            // Debug.Log(zoomInput);
            // Debug.Log(clampedZoomInput);
            isCameraScrolling = false; 
        }
    }


    private void UpdateZoomLevel()
    {
        if (currentMiddleRigRadius == newMiddleRigRadius) {return; }

        currentMiddleRigRadius = Mathf.Lerp(currentMiddleRigRadius, newMiddleRigRadius, zoomAcceleration * Time.deltaTime);
        currentMiddleRigRadius = Mathf.Clamp(currentMiddleRigRadius, zoomInnerRange, zoomOuterRange);

        playerObjectCameraController.vc.m_Orbits[1].m_Radius = currentMiddleRigRadius;
        playerObjectCameraController.vc.m_Orbits[0].m_Height = playerObjectCameraController.vc.m_Orbits[1].m_Radius;
        playerObjectCameraController.vc.m_Orbits[2].m_Height = playerObjectCameraController.vc.m_Orbits[1].m_Radius;
    }


    public void CameraZoomSetter(float clampedZoomInput)
    {

        if (clampedZoomInput == 0 ) { return; }

        if (clampedZoomInput < 0 ) 
        {
            newMiddleRigRadius = currentMiddleRigRadius + zoomSpeed;
        }

        if (clampedZoomInput > 0 ) 
        {
            newMiddleRigRadius = currentMiddleRigRadius - zoomSpeed;
        }
    }

    // Not In Use
    
    public void Spawn(InputAction.CallbackContext context)  // I'll just make a button for spawn, this can be anything
    {
        if(context.performed == true) 
        {
            
        }

        if(context.performed == false) 
        {
            return;
        }
    }
}