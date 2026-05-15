#nullable enable
// Tech-spec 02 § EnemyDefinition.role. Mirrors GDD 05-enemies.md role taxonomy.

namespace Brave.Gameplay.Definitions
{
    /// <summary>
    /// Enemy archetype role. Used by the wave driver to select variants per biome
    /// and by AI ticker to choose a behavior LUT entry.
    /// </summary>
    public enum EnemyRole
    {
        Swarmer = 0,
        Tank    = 1,
        Ranged  = 2,
        Elite   = 3,
        // ADR-0020: data/balance/enemies.json ships "role": "boss" for old-boar-king.
        // Without this value the BalanceJsonImporter silently defaulted it to Swarmer.
        Boss    = 4,
    }
}
