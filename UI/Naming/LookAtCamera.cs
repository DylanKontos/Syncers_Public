using FishNet.Object;
using UnityEngine;
using System.Collections;


public class LookAtCamera : NetworkBehaviour  // Makes the canvas LOOK AT each players instance of these camera.
{
    public Camera mainCam;
    public Camera targetCam;

    vcTargetCamera _vcTargetCamera;

    // private Camera activeCam; // Tried setting a bool, but would constantly flip true/false as multiple clients are accessing cameras?
    // Ownership checks did not stop this.
    // Coroutine best solution.

    public override void OnStartClient()
    {
        mainCam = FindObjectOfType<Camera>(); // RETURNING as target cam
        _vcTargetCamera = FindAnyObjectByType<vcTargetCamera>();
        targetCam = _vcTargetCamera.GetComponent<Camera>();

        StartCoroutine(LookAtMainCam());
        // activeCam = mainCam; // bool that would flip
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        StopAllCoroutines();
    }

    // public void Update()
    // {
    //     Debug.Log(mainCam);
    //     Debug.Log(targetCam);
    // }

    IEnumerator LookAtMainCam()
    {
        while (true)
        {
            transform.LookAt(targetCam.transform);
            yield return null; // Wait until next frame
        }
    }

    IEnumerator LookAtTargetCam()
    {
        while (true)
        {
            transform.LookAt(targetCam.transform);
            yield return null; // Wait until next frame
        }
    }
}


    // old code with flipping bool

    // private void Update()
    // {
    //     if (!IsOwner) return; 

    //     Debug.Log(activeCam);   // targetCam AND a seperate debug line mainCam simultaneously 

    //     if (activeCam != null)
    //     {
    //         transform.LookAt(activeCam.transform);
    //     }
    // }

    // public void SetActiveCamera(bool isTargetCameraActive)
    // {
    //     if (!IsOwner) return; 

    //     Debug.Log(isTargetCameraActive); // sets correctly
    //     activeCam = isTargetCameraActive ? targetCam : mainCam;
    // }

    // private void Update()
    // {
    //     Debug.Log(targetCamera);

    //     if (mainCam == null)
    //     {
    //         mainCam = FindObjectOfType<Camera>(); 
    //     }

    //     if (mainCam == null) return; 

    //     transform.LookAt(mainCam.transform);



    //     // Debug.Log(targetCamActive);

    //     // Camera cameraToLookAt = targetCamActive ? targetCamera : mainCam;

    //     // transform.LookAt(cameraToLookAt.transform);
    // }

    // public void SetTargetCamActiveTrue()
    // {
    //      targetCamActive = true;
    // }

    // public void SetTargetCamActiveFalse()
    // {
    //      targetCamActive = false;
    // }