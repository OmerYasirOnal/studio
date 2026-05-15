// Brave Bunny — Systems/LiveOps/BiomeRegistry
//
// Enum-driven biome catalog. Maps the launch biome lineup
// (Meadow / Beach / Forest / Cavern / Snow — see docs/02-gdd/06-biomes.md)
// to the data assets the runtime needs for each: slug, display name, the
// owning waves.json resource path, and the WaveDefinition asset path it
// imports into.
//
// Why this lives in Systems/LiveOps and not Gameplay/Run:
//   - The Run scene is biome-agnostic (RunController takes a WaveDefinition
//     by reference; it does not pick the biome). Biome selection is a
//     systems-level decision driven by the meta-loop unlock state and the
//     Loadout screen.
//   - Adding/disabling biomes during live-ops should not require recompiling
//     gameplay code — the enum + table here is the single edit point.
//
// What it does NOT do (yet):
//   - Load WaveDefinition at runtime. The current pipeline imports waves.json
//     into a WaveDefinition .asset via BalanceJsonImporter (editor-only); this
//     registry exposes the canonical .asset path so a future runtime loader
//     (Resources, Addressables, or direct injection) can resolve it.
//   - Gate by unlock-state. That belongs in ProgressionService — this registry
//     is a static catalog.
//
// Vertical-slice status (Wave 9):
//   * Meadow:  shipped (boss = Old Boar King)
//   * Beach:   wired (boss = crab-captain, deferred — single launch boss policy)
//   * Cavern:  wired (boss = sneaky-cave-mole, deferred)
//   * Forest:  scaffolded only (layout.md, no waves.json)
//   * Snow:    scaffolded only (layout.md, no waves.json)
//
// Spec refs:
//   * docs/02-gdd/06-biomes.md          — biome lineup + difficulty multipliers
//   * docs/09-level-design/01-biomes/   — per-biome waves.json + layout.md
//   * docs/06-tech-spec/02-data-model.md§ BiomeDefinition / WaveDefinition

#nullable enable

using System;
using System.Collections.Generic;

namespace Brave.Systems.LiveOps
{
    /// <summary>
    /// Launch biome lineup. Ordering matches the meta-loop unlock cadence in
    /// <c>docs/02-gdd/02-meta-loop.md</c> — Meadow first, Snow last.
    /// </summary>
    public enum BiomeId
    {
        Meadow = 0,
        Beach  = 1,
        Forest = 2,
        Cavern = 3,
        Snow   = 4,
    }

    /// <summary>
    /// Designer-facing catalog row for a biome. Pure data — no Unity dependencies
    /// so EditMode tests can exercise the registry without Resources/Addressables.
    /// </summary>
    public readonly struct BiomeCatalogEntry : IEquatable<BiomeCatalogEntry>
    {
        public readonly BiomeId id;
        public readonly string slug;          // kebab-case, matches waves.json "biome" field
        public readonly string displayName;
        public readonly string wavesJsonRelativePath;  // relative to data/balance/.. → docs/09-level-design/01-biomes/<slug>/waves.json
        public readonly string waveAssetPath;          // Project-relative path of the imported WaveDefinition .asset
        public readonly string bossSlug;               // boss enemy id referenced from waves.json (may be deferred)
        public readonly bool available;                // false → biome is scaffolded only (no waves.json yet)

        public BiomeCatalogEntry(
            BiomeId id,
            string slug,
            string displayName,
            string wavesJsonRelativePath,
            string waveAssetPath,
            string bossSlug,
            bool available)
        {
            this.id = id;
            this.slug = slug;
            this.displayName = displayName;
            this.wavesJsonRelativePath = wavesJsonRelativePath;
            this.waveAssetPath = waveAssetPath;
            this.bossSlug = bossSlug;
            this.available = available;
        }

        public bool Equals(BiomeCatalogEntry other) => id == other.id;
        public override bool Equals(object? obj) => obj is BiomeCatalogEntry o && Equals(o);
        public override int GetHashCode() => (int)id;
    }

    /// <summary>
    /// Static, allocation-free biome catalog. <see cref="ResolveBy(BiomeId)"/> is
    /// O(1) (array index); <see cref="ResolveBySlug(string)"/> is O(N=5).
    /// </summary>
    public static class BiomeRegistry
    {
        // ---- Repo-relative paths (no magic strings sprinkled at call sites) ----
        // Path layout — single source of truth so tests can validate both
        // wavesJsonRelativePath ("docs/09-level-design/01-biomes/<slug>/waves.json"
        // resolved from data/balance/..) and the imported .asset under
        // Assets/_Brave/Data/Balance/Wave_<slug>.asset.
        private const string WavesJsonRoot = "docs/09-level-design/01-biomes";
        private const string WavesJsonFile = "waves.json";
        private const string WaveAssetRoot = "Assets/_Brave/Data/Balance";
        private const string WaveAssetPrefix = "Wave_";
        private const string WaveAssetExt = ".asset";

        private static readonly BiomeCatalogEntry[] _entries = BuildCatalog();

        private static BiomeCatalogEntry[] BuildCatalog()
        {
            return new[]
            {
                MakeEntry(BiomeId.Meadow, "meadow", "Meadow", "old-boar-king",   available: true),
                MakeEntry(BiomeId.Beach,  "beach",  "Beach",  "crab-captain",    available: true),
                MakeEntry(BiomeId.Forest, "forest", "Forest", "mama-oak",        available: false),
                MakeEntry(BiomeId.Cavern, "cavern", "Cavern", "sneaky-cave-mole", available: true),
                MakeEntry(BiomeId.Snow,   "snow",   "Snow",   "frost-bear",      available: false),
            };
        }

        private static BiomeCatalogEntry MakeEntry(
            BiomeId id, string slug, string displayName, string bossSlug, bool available)
        {
            string wavesJsonRel = $"{WavesJsonRoot}/{slug}/{WavesJsonFile}";
            string waveAsset    = $"{WaveAssetRoot}/{WaveAssetPrefix}{slug}{WaveAssetExt}";
            return new BiomeCatalogEntry(
                id, slug, displayName, wavesJsonRel, waveAsset, bossSlug, available);
        }

        /// <summary>All registered biomes (including scaffolded-only ones).</summary>
        public static IReadOnlyList<BiomeCatalogEntry> All => _entries;

        /// <summary>How many biomes have <c>available == true</c>.</summary>
        public static int AvailableCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < _entries.Length; i++)
                    if (_entries[i].available) n++;
                return n;
            }
        }

        /// <summary>Direct lookup by enum. Throws <see cref="ArgumentOutOfRangeException"/>
        /// if <paramref name="id"/> is not a defined enum value.</summary>
        public static BiomeCatalogEntry ResolveBy(BiomeId id)
        {
            int idx = (int)id;
            if (idx < 0 || idx >= _entries.Length)
                throw new ArgumentOutOfRangeException(
                    nameof(id), id, $"BiomeId out of range [0,{_entries.Length - 1}]");
            return _entries[idx];
        }

        /// <summary>Lookup by kebab-case slug. Returns <c>false</c> when no match.
        /// No allocations.</summary>
        public static bool TryResolveBySlug(string? slug, out BiomeCatalogEntry entry)
        {
            if (!string.IsNullOrEmpty(slug))
            {
                for (int i = 0; i < _entries.Length; i++)
                {
                    if (string.Equals(_entries[i].slug, slug, StringComparison.Ordinal))
                    {
                        entry = _entries[i];
                        return true;
                    }
                }
            }
            entry = default;
            return false;
        }

        /// <summary>Convenience throw-on-miss variant of <see cref="TryResolveBySlug"/>.</summary>
        public static BiomeCatalogEntry ResolveBySlug(string slug)
        {
            if (!TryResolveBySlug(slug, out var entry))
                throw new KeyNotFoundException($"BiomeRegistry: no biome with slug '{slug}'");
            return entry;
        }

        /// <summary>The relative path (from <c>data/balance/..</c>) to the biome's
        /// waves.json. Path returned even for scaffolded-only biomes — caller decides
        /// whether to honor the <see cref="BiomeCatalogEntry.available"/> flag.</summary>
        public static string WavesJsonPathFor(BiomeId id) => ResolveBy(id).wavesJsonRelativePath;

        /// <summary>The project-relative path to the imported WaveDefinition .asset
        /// for the biome. Editor / Resources lookup goes through this string so the
        /// asset-path convention lives in exactly one place.</summary>
        public static string WaveAssetPathFor(BiomeId id) => ResolveBy(id).waveAssetPath;
    }
}
