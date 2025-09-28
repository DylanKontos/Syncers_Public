using FishNet.Object;
using FishNet.Object.Prediction;
using GameKit.Dependencies.Utilities;
using UnityEngine;

public class PhysicsObject : NetworkBehaviour
{
    public PredictionRigidbody rb;
    
    private void Awake()
    {
        rb = ObjectCaches<PredictionRigidbody>.Retrieve();
        rb.Initialize(GetComponent<Rigidbody>());
    }
    
    private void OnDestroy()
    {
        ObjectCaches<PredictionRigidbody>.StoreAndDefault(ref rb);
    }
}