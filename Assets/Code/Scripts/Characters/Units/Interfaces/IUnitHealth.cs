namespace AF.TS.Characters
{
    public interface IUnitHealth : IUnitCommon
    {
        void TakeDamage(float amount);
        void Heal(float amount);
        bool IsDead { get; }
    }

}
