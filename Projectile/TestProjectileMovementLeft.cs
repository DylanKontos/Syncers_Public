using UnityEngine;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using GameKit.Dependencies.Utilities;

[RequireComponent(typeof(PhysicsObject))]
public class TestProjectileMovementLeft : NetworkBehaviour
{
    private PredictionRigidbody rb;
    [SerializeField] private bool reconcile = true;

    public struct ReplicateData : IReplicateData
    {
        public ReplicateData(uint unused) : this() {}
        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public struct ReconcileData : IReconcileData
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
        base.TimeManager.OnPostTick += TimeManager_OnPostTick;
        rb = GetComponent<PhysicsObject>().rb;
        rb.Velocity(transform.right * -80);
        rb.Simulate();
    }

    // public void Move()
    // {
    //     rb.Velocity(transform.right * -80);
    // }

    private void Awake()
    {
        rb = GetComponent<PhysicsObject>().rb;
    }

    public override void OnStopNetwork()
    {
        base.TimeManager.OnPostTick -= TimeManager_OnPostTick;
    }


    private void TimeManager_OnPostTick()
    {
        RunInputs(default);
        if (reconcile) CreateReconcile();
    }
    
    [Replicate]
    private void RunInputs(ReplicateData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {

    }
    
    public override void CreateReconcile()
    {
        ReconcileData rd = new ReconcileData(rb);
        ReconcileState(rd);
    }

    [Reconcile]
    private void ReconcileState(ReconcileData data, Channel channel = Channel.Unreliable)
    {
        rb.Reconcile(data.Rb);
    }
}
