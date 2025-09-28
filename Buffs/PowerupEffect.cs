using UnityEngine;

// abastract - You cant make an instance of PowerupEffect.
// You can inherit from it... 
public abstract class PowerupEffect : ScriptableObject
{
    public abstract void Apply(GameObject target);
}
