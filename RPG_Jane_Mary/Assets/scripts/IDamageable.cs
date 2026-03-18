public interface IDamageable
{
    void TakeDamage(float physicalDamage, float magicDamage);
    float CurrentHealth { get; }
    float MaxHealth { get; }
}