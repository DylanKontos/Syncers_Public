using UnityEngine;

[CreateAssetMenu(menuName = "Powerups/Healthbuff")]
public class HealthBuff : PowerupEffect
{
    // public int amount; 

    public override void Apply(GameObject target)
    {
        // target.GetComponent<ControlledShip>().RestoreHealth();
        ControlledShip controlledShip = target.GetComponentInParent<ControlledShip>();
        controlledShip.RestoreHealth();
    }
}
