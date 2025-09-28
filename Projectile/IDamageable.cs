namespace Syncers.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(DamageData damageData);
    }

    public class DamageData
    {
        public int damageAmount;
        public Player damageSourcePlayer;

        public DamageData(int amount, Player sourcePlayer)
        {
            damageAmount = amount;
            damageSourcePlayer = sourcePlayer;
        }
    }
}
