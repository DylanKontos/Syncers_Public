using UnityEngine;
using Cinemachine;
using System.Collections;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// ///////////////////////////////////////////////////////// CAUTION /////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////// PlayerObjectCameraController is a component on the existing universal camera, its not actually on the player.  /////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


public class PlayerObjectCameraController : MonoBehaviour        // CAMERA logic is called in PlayerObjectMovement

{
    Camera mainCam;  // main camera on initial object
    CinemachineInputProvider cinemachineInputProvider;
    public CinemachineFreeLook vc;
    private CinemachineVirtualCamera vcTarget;
    public CinemachineTargetGroup targetGroup;

    // obj thats passed in from FirstObjectNotifier
    public Transform playerObject; 
    private GameObject lookAtCamera;


    // [SerializeField] 
    // GameObject mr;   // For the camera to look at the mesh renderer but its just looking at graphical object now. No difference. 
    // mr is child of graphobj

    // FreeLookCamera recenter variables for freelook
    public bool recenter;
    public float recenterTime = 0.5f;

    // VirtualCamera or Target Camera Variables
    private Vector3 offset; // The initial offset from the obj
    private Transform currentTarget;

    // better for camera to follow the GraphicalObject of the controlledShip/obj 
    GameObject GraphicalObject;

    bool isFirstSpawn = true;


	public void Awake()
    {
        FirstObjectNotifier.OnFirstObjectSpawned += FirstObjectNotifier_OnFirstObjectSpawned;

        mainCam = GetComponent<Camera>();
        vc = GetComponent<CinemachineFreeLook>();
        targetGroup = vc.GetComponent<CinemachineTargetGroup>();
        cinemachineInputProvider = vc.GetComponent<CinemachineInputProvider>();

        // vcTarget was not functioning as a component, a seperate camera gameObject was needed (troubles switching between freelook/virtual)
        GameObject childObject = transform.Find("vcTargetCamera").gameObject;
        vcTarget = childObject.GetComponent<CinemachineVirtualCamera>();

        vc.enabled = false;
    }


    private void FirstObjectNotifier_OnFirstObjectSpawned(Transform obj)
    {
        StartCoroutine(StartCamera(obj));

        // vc.enabled = true;

        // GraphicalObject = obj.transform.Find("GraphicalObject").gameObject; // used by target cam (up close) and no jitter
        // // mr = GraphicalObject.transform.Find("MeshRenderer").gameObject; // no difference...

        // vc.Follow = GraphicalObject.transform; 
        // vc.LookAt = GraphicalObject.transform;  // was playerObject or obj

        // // vc.Follow = mr.transform; 
        // // vc.LookAt = mr.transform;  // was playerObject or obj

        // playerObject = obj; // obj now class-wide variable as playerObject

        // // vc.m_YAxis.Value = 0.65f; // ON SPAWN // Set to behind controlledShip, default is underneath...
        // vc.m_YAxis.Value = 0.68f;

        // RecenterCamera();  // Recenter at Spawn to face the correct direction.
    }

    private IEnumerator StartCamera(Transform obj)
    {
        while (GraphicalObject == null)
        {
            GraphicalObject = obj.transform.Find("GraphicalObject")?.gameObject;
            if (GraphicalObject == null)
            {
                yield return new WaitForSeconds(1);
            }
        }

        vc.enabled = true;

        // used by target cam (up close) and no jitter
        // GraphicalObject found earlier
        vc.Follow = GraphicalObject.transform; 
        vc.LookAt = GraphicalObject.transform;  // was playerObject or obj

        // obj now class-wide variable as playerObject
        playerObject = obj; 

        // ON SPAWN // Set to behind controlledShip, default is underneath...
        vc.m_YAxis.Value = 0.68f;

        // Recenter at Spawn to face the correct direction.
        RecenterCamera();  
    }

    public void ChangeSensitivity(float sensitivity)
    {
        vc.m_YAxis.m_MaxSpeed = sensitivity / 70; // Adjust as needed to fit your setup
        vc.m_XAxis.m_MaxSpeed = sensitivity * 2;
        // Debug.Log(sensitivity);

        // X axis == 300
        // y axis = 0.2

        // Make slider
    }

    public void SetCameraTarget(Transform target)
    {
        vc.LookAt = target;

        vc.enabled = false;
        mainCam.enabled = false;
        vcTarget.enabled = true;
        vcTarget.Follow = GraphicalObject.transform;    // WAS playerObject 
        // vcTarget.Follow = playerObject;

        vcTarget.LookAt = target;
        currentTarget = target; // target now class-wide variable as currentTarget

        float cameraRadius = vc.m_Orbits[1].m_Radius;
        // Set vcTarget.Radius to vc.Radius




        // StartCoroutine(LookAtTarget(target));  // coroutine also possible...
    }

    private void LateUpdate()
    {
        if (playerObject == null) { return; }

        if (currentTarget != null && vc.enabled == false )
        {
            offset = vcTarget.transform.position - playerObject.transform.position;

            vcTarget.transform.position = playerObject.transform.position + offset;
            vcTarget.transform.LookAt(currentTarget.transform);

            // TO:DO add zoom that saves.
                // --> read input from playerObject and apply the same degree, should transfer to freelook fine
        }

        if (playerObject == currentTarget)  // deprecated as playerObject will never be targetted, but just incase..
        {
            vc.enabled = true;   //  !!! BREAK out of vcTarget !!!!
            vcTarget.enabled = false;
        }
    }

    public void TurnOffTargetCameraMode()  // update to a toggle with a flag
    {
        vc.enabled = true;   //  !!! BREAK out of vcTarget !!!!
        vcTarget.enabled = false;

        vc.LookAt = GraphicalObject.transform; // WAS originally playerObject;
    }

    public void RecenterCamera()  // occurs when mouse1 isn't held // WoWcam
    {
        cinemachineInputProvider.enabled = false; 
        vc.m_RecenterToTargetHeading.m_enabled = true;

        Invoke("SetIsFirstSpawn", 0.2f);

    }

    private void SetIsFirstSpawn()
    {
        isFirstSpawn = false; 
    }


    public void LockCamera() // Occurs when mouse1 isnt held 14/10/24
    {
        if (isFirstSpawn == false)
        {
            cinemachineInputProvider.enabled = false; 
            vc.m_RecenterToTargetHeading.m_enabled = false;
        }

        else return;
    }

    public void ActivateFreeLookCamera()
    {
        cinemachineInputProvider.enabled = true;
        vc.m_RecenterToTargetHeading.m_enabled = false;
    }
}

    // coroutine was moved to LateUpdate();
    // private IEnumerator LookAtTarget(Transform target)
    // {
    //     while (vc.enabled == false)
    //     {
    //         offset = vcTarget.transform.position - obj.transform.position;
    //         vcTarget.transform.position = obj.transform.position + offset;
    //         vcTarget.transform.LookAt(target.transform);

    //         // Check for the input condition
    //         if (Input.GetKeyDown(KeyCode.X)) // Replace with your actual input condition
    //         {
    //             break; // Exit the loop
    //         }

    //         yield return null; // This will make the coroutine wait for the next frame before continuing the loop
    //     }
    // }

    // Target groups. Didn't work but here is the code...

    // targetGroup.m_Targets = new CinemachineTargetGroup.Target[]
    // {
    //     new CinemachineTargetGroup.Target { target = obj.transform, weight = 1 },
    //     new CinemachineTargetGroup.Target { target = target.transform, weight = 1 }
    // };

    // // Set the LookAt of the CinemachineFreeLook to the target group
    // vc.LookAt = targetGroup.transform;

        // float xAxisLockCamera;        
        // float yAxisLockCamera;

        // // save most recent values
        // xAxisLockCamera = vc.m_XAxis.Value;
        // yAxisLockCamera = vc.m_YAxis.Value;
        
        // // lock at stored recent values
        // vc.m_XAxis.Value = xAxisLockCamera;
        // vc.m_YAxis.Value = yAxisLockCamera;