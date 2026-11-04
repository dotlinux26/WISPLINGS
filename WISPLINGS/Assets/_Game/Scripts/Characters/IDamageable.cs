namespace GhostSurvival.Characters
{
    /// <summary>
    /// Simple interface for entities that can take damage.
    /// Designed cleanly for future combat and multiplayer expansions.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float amount);
        bool IsAlive { get; }
    }
}

