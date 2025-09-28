using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/ShieldBuff")]
public class ShieldBuff : PowerupEffect
{
    // public int amount; 

    public override void Apply(GameObject target)
    {
        // target.GetComponent<ControlledShip>().RestoreHealth();
        ControlledShip controlledShip = target.GetComponentInParent<ControlledShip>();
        controlledShip.AddShield();
    }
}
