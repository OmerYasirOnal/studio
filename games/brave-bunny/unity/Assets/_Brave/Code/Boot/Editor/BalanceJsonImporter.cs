#nullable enable
// Editor-only — generates ScriptableObject .asset stubs from data/balance/*.json.
// Run via Menu: Brave > Generate Balance SOs from JSON.
//
// Per ADR-0008 + tech-spec 02-data-model.md, balance JSON is the source of truth.
// This importer creates the matching SO assets under Assets/_Brave/Data/Balance/
// at edit time. Re-runnable; existing assets are updated in place via SerializedObject.

#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Brave.Gameplay.Combat.Archetypes;
using Brave.Gameplay.Definitions;

namespace Brave.Boot.Editor
{
    public static class BalanceJsonImporter
    {
        // Relative paths from Application.dataPath (= <project>/Assets/).
        // The balance JSON lives at <project>/../../data/balance relative to Assets/,
        // i.e. two directories up from Assets/ → the game root → data/balance.
        // (Application.dataPath = .../brave-bunny/unity/Assets  →  ../../data/balance
        //  resolves to .../brave-bunny/data/balance which matches the repo layout.)
        private const string BalanceDataDir = "../../data/balance";
        private const string OutputDir = "Assets/_Brave/Data/Balance";
        // ADR-0020: archetype-config sidecar SOs live alongside WeaponDefinition assets
        // under an Archetypes subfolder so the Project window groups them clearly.
        private const string ArchetypeOutputDir = OutputDir + "/Archetypes";
        // Weapon levels are EXACTLY 5 (tech-spec 02 / WeaponDefinition.OnValidate).
        private const int WeaponLevelCount = 5;

        [MenuItem("Brave/Generate Balance SOs from JSON")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(OutputDir);
            Directory.CreateDirectory(ArchetypeOutputDir);

            int created = 0;
            int updated = 0;

            ImportCharacters(ref created, ref updated);
            ImportWeapons(ref created, ref updated);
            ImportPassives(ref created, ref updated);
            ImportEnemies(ref created, ref updated);
            // Note: xp-curve, drops, economy, feel are smaller scalar/table JSONs;
            // they import into single SO instances rather than collections.
            ImportSingle("xp-curve.json", "XpCurve", ref created, ref updated);
            ImportSingle("drops.json", "Drops", ref created, ref updated);
            ImportSingle("economy.json", "Economy", ref created, ref updated);
            ImportSingle("feel.json", "Feel", ref created, ref updated);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[balance-importer] OK — created {created}, updated {updated}");
        }

        private static JToken? LoadJson(string filename)
        {
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, BalanceDataDir, filename));
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[balance-importer] skip: {path} not found");
                return null;
            }
            try
            {
                return JToken.Parse(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogError($"[balance-importer] parse error in {filename}: {e.Message}");
                return null;
            }
        }

        private static T LoadOrCreate<T>(string assetPath) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existing != null) return existing;
            var so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, assetPath);
            return so;
        }

        // --- Per-type importers -------------------------------------------------

        private static void ImportCharacters(ref int created, ref int updated)
        {
            var root = LoadJson("characters.json");
            if (root == null) return;
            // characters.json is an array (or object with "characters" key — handle both)
            var list = (root is JArray arr) ? (JArray)arr : (JArray)(root["characters"] ?? new JArray());

            // File-level calibration constants. Per docs/10-balance/00-formulas.md §3 (movement)
            // and §1 (damage / HP scaling): per-character mults are applied to these baselines.
            // base_move_units_per_sec is REQUIRED for baseMoveSpeed math; default to Bunny anchor 4.5.
            var rootObj = root as JObject;
            float baseMoveUnitsPerSec = rootObj?.Value<float?>("base_move_units_per_sec") ?? 4.5f;
            // Note: hp_base is per-character (absolute HP), not a file-level constant — see schema.

            foreach (JObject entry in list.OfType<JObject>())
            {
                var slug = entry.Value<string>("id") ?? entry.Value<string>("slug");
                if (string.IsNullOrEmpty(slug)) continue;

                var assetPath = $"{OutputDir}/Char_{slug}.asset";
                var existed = File.Exists(assetPath);
                var so = LoadOrCreate<CharacterDefinition>(assetPath);

                using var serialized = new SerializedObject(so);
                ApplyField(serialized, "slug", slug);
                ApplyField(serialized, "displayName", entry.Value<string>("display_name"));
                ApplyField(serialized, "signatureTypeName", entry.Value<string>("signature_token"));
                ApplyField(serialized, "unlockStarCost", entry.Value<int?>("unlock_star_cost") ?? 0);

                // --- CharacterStats (baseStats sub-struct) -----------------------
                // Per docs/10-balance/00-formulas.md:
                //   §3 movement: move_speed = base_move × character.move_mult
                //   §1 damage:   character_dmg_mult comes from characters.json → dmg_mult
                //   §2 crit:     crit_rate, crit_damage are per-character bases
                //   §4 magnet:   magnet_mult is per-character; base_magnet lives on PlayerMover/Magnet, not in CharacterStats
                // hp_base in JSON is the absolute level-1 HP (per schema § "hp_base [50,250]"),
                // so baseStats.baseHP = hp_base directly (no multiplier — it IS the baseline).
                float hpBase    = entry.Value<float?>("hp_base") ?? 0f;
                float moveMult  = entry.Value<float?>("move_mult") ?? 1f;
                float dmgMult   = entry.Value<float?>("dmg_mult") ?? 1f;
                float critRate  = entry.Value<float?>("crit_rate") ?? 0f;
                float critDmg   = entry.Value<float?>("crit_damage") ?? 1f;
                float magnetM   = entry.Value<float?>("magnet_mult") ?? 1f;
                float xpGemBon  = entry.Value<float?>("xp_gem_value_bonus") ?? 0f;

                ApplyField(serialized, "baseStats.baseHP",           hpBase);
                ApplyField(serialized, "baseStats.baseMoveSpeed",    baseMoveUnitsPerSec * moveMult);
                ApplyField(serialized, "baseStats.damageMultiplier", dmgMult);
                ApplyField(serialized, "baseStats.critRate",         critRate);
                ApplyField(serialized, "baseStats.critDamage",       critDmg);
                ApplyField(serialized, "baseStats.magnetMultiplier", magnetM);
                ApplyField(serialized, "baseStats.xpGemValueBonus",  xpGemBon);

                // --- UnlockCondition (meta-progression) --------------------------
                // Inlined as raw scalar fields on CharacterDefinition.UnlockConditionData
                // (asmdef layering — see CharacterDefinition.cs header). Empty / "none"
                // type marks the slug as a starter.
                var unlock = entry["unlock_condition"] as JObject;
                if (unlock != null)
                {
                    ApplyField(serialized, "unlockCondition.type",          unlock.Value<string>("type") ?? string.Empty);
                    ApplyField(serialized, "unlockCondition.wave",          unlock.Value<int?>("wave") ?? 0);
                    ApplyField(serialized, "unlockCondition.boss",          unlock.Value<string>("boss") ?? string.Empty);
                    ApplyField(serialized, "unlockCondition.runs",          unlock.Value<int?>("runs") ?? 0);
                    ApplyField(serialized, "unlockCondition.withCharacter", unlock.Value<string>("with_character") ?? string.Empty);
                    ApplyField(serialized, "unlockCondition.stars",         unlock.Value<int?>("stars") ?? 0);
                }
                else
                {
                    // No condition declared in JSON → treat as starter (type=none).
                    ApplyField(serialized, "unlockCondition.type", "none");
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(so);

                if (existed) updated++; else created++;
            }
        }

        private static void ImportWeapons(ref int created, ref int updated)
        {
            var root = LoadJson("weapons.json");
            if (root == null) return;
            var list = (root is JArray arr) ? (JArray)arr : (JArray)(root["weapons"] ?? new JArray());
            foreach (JObject entry in list.OfType<JObject>())
            {
                var slug = entry.Value<string>("id") ?? entry.Value<string>("slug");
                if (string.IsNullOrEmpty(slug)) continue;

                var assetPath = $"{OutputDir}/Weapon_{slug}.asset";
                var existed = File.Exists(assetPath);
                var so = LoadOrCreate<WeaponDefinition>(assetPath);

                using var serialized = new SerializedObject(so);
                ApplyField(serialized, "slug", slug);
                ApplyField(serialized, "displayName", entry.Value<string>("display_name"));
                // archetype is an enum on the SO (WeaponArchetype). The JSON ships a kebab-case
                // string ("projectile" / "area" / etc.). enumValueIndex mapping is done by name
                // in ApplyEnumField below; for now we leave it (already-imported assets retain
                // their enum index unless changed in the Inspector).
                ApplyEnumByJsonString(serialized, "archetype", entry.Value<string>("archetype"));

                // --- WeaponDefinition.levels[5] — per-level rows ---------------------
                // Per docs/10-balance/00-formulas.md §1 (damage formula):
                //   damage = dmg_base × level_mult[level]   (other multipliers applied at runtime)
                // Per weapons.schema.md:
                //   rate_ms (ms-between-fires) → WeaponLevelData.fireRate is SECONDS (seconds-between-fires)
                //   range_units → WeaponLevelData.range (game units)
                //   projectiles_base → WeaponLevelData.projectiles (count per fire; 0 for aura)
                //   level_mult[i] is the DMG multiplier at L(i+1); level_mult[0] = 1.00 = L1 baseline.
                //
                // L2..L5 perks (rate_ms / projectiles / range deltas) live in level_perks[] and ARE applied
                // per-level here when the perk is one of: "rate_ms", "projectiles", "range_units",
                // "range_units_delta". Other perks (chain/bounce/pierce/slow_pct/etc.) are runtime
                // mechanics owned by gameplay-engineer and stay out of this static SO row.
                float dmgBase   = entry.Value<float?>("dmg_base") ?? 0f;
                int   rateMs0   = entry.Value<int?>("rate_ms") ?? 0;
                float range0    = entry.Value<float?>("range_units") ?? 0f;
                int   projs0    = entry.Value<int?>("projectiles_base") ?? 0;
                var   levelMult = entry["level_mult"] as JArray;
                var   perks     = entry["level_perks"] as JArray;

                // Carry-forward (per-level) state — perks at level N modify the state used from N onward.
                int   curRateMs = rateMs0;
                float curRange  = range0;
                int   curProjs  = projs0;

                for (int i = 0; i < 5; i++)
                {
                    int level1Based = i + 1;
                    float mult = (levelMult != null && i < levelMult.Count) ? levelMult[i].Value<float>() : 1f;

                    // Apply any perk targeting THIS level (level 2..5 only — L1 is the baseline).
                    if (perks != null && level1Based >= 2)
                    {
                        foreach (var p in perks.OfType<JObject>())
                        {
                            if ((p.Value<int?>("level") ?? -1) != level1Based) continue;
                            var perkName = p.Value<string>("perk");
                            switch (perkName)
                            {
                                case "rate_ms":
                                    // value is the NEW absolute rate_ms after this level.
                                    curRateMs = p.Value<int?>("value") ?? curRateMs;
                                    break;
                                case "projectiles":
                                    // value is the DELTA (+1 per occurrence per schema).
                                    curProjs += p.Value<int?>("value") ?? 0;
                                    break;
                                case "range_units":
                                    // value is the NEW absolute range (Whirligig L4: 3.0).
                                    curRange = p.Value<float?>("value") ?? curRange;
                                    break;
                                case "range_units_delta":
                                    // additive delta (Honey Aura L2: +0.5).
                                    curRange += p.Value<float?>("value") ?? 0f;
                                    break;
                                // Other perks are runtime-mechanic and don't belong in static row fields.
                            }
                        }
                    }

                    // Per-level SO row — SerializedProperty array path syntax.
                    var rowPath = $"levels.Array.data[{i}]";
                    ApplyField(serialized, $"{rowPath}.damage",       dmgBase * mult);
                    ApplyField(serialized, $"{rowPath}.fireRate",     curRateMs / 1000f);  // ms → seconds
                    ApplyField(serialized, $"{rowPath}.range",        curRange);
                    ApplyField(serialized, $"{rowPath}.projectiles",  curProjs);
                    // upgradeFlavor is designer copy; left at default (empty) — no JSON source.
                }

                // ADR-0020: build / refresh the archetype-config sidecar SO and
                // wire it onto WeaponDefinition.archetypeConfig BEFORE applying
                // the SerializedObject (so the objectReferenceValue persists).
                ImportWeaponArchetypeConfig(so, slug!, entry, serialized);

                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(so);

                if (existed) updated++; else created++;
            }
        }

        // --- ADR-0020 archetype dispatch ----------------------------------------
        // Picks the concrete WeaponArchetypeConfig subclass from the weapon JSON.
        // Disambiguates the JSON "archetype" enum string with key-presence checks
        // because the JSON "area" archetype covers Daisy Mine, Thunder Cloud, AND
        // Cob Mortar — each of which needs a different SO subclass.
        private static System.Type? ResolveArchetypeType(string? archetypeStr, JObject entry)
        {
            if (string.IsNullOrEmpty(archetypeStr)) return null;

            // Key-presence disambiguators (ADR-0020 §Decision).
            bool hasArmTime      = entry["arm_time_ms"]       != null;
            bool hasCloudLife    = entry["cloud_lifetime_ms"] != null;
            bool hasSplashUnits  = entry["splash_units_base"] != null;
            bool hasSlowPct      = entry["slow_pct_base"]     != null;
            bool hasLifetimeMs   = entry["lifetime_ms"]       != null;

            string a = archetypeStr.ToLowerInvariant();

            // JSON "area" covers mine / cloud / splash-projectile — disambiguate via key-presence.
            if (a == "area")
            {
                if (hasArmTime)     return typeof(MineArchetypeConfig);
                if (hasCloudLife)   return typeof(CloudArchetypeConfig);
                if (hasSplashUnits) return typeof(SplashProjectileArchetypeConfig);
                return null;
            }

            if (a == "aura")        return typeof(AuraArchetypeConfig);
            if (a == "summon")      return typeof(SummonArchetypeConfig);
            if (a == "projectile")  return typeof(ProjectileArchetypeConfig);
            // "utility-beam" → Sunbeam.
            if (a.StartsWith("utility-beam") || a == "beam") return typeof(BeamArchetypeConfig);

            // Fallback by field presence (handles archetypes shipped without an exact-match string).
            if (hasArmTime)     return typeof(MineArchetypeConfig);
            if (hasCloudLife)   return typeof(CloudArchetypeConfig);
            if (hasSplashUnits) return typeof(SplashProjectileArchetypeConfig);
            if (hasSlowPct)     return typeof(AuraArchetypeConfig);
            if (hasLifetimeMs)  return typeof(SummonArchetypeConfig);
            return null;
        }

        private static void ImportWeaponArchetypeConfig(
            WeaponDefinition weaponSo, string slug, JObject entry,
            SerializedObject weaponSerialized)
        {
            var archetypeType = ResolveArchetypeType(entry.Value<string>("archetype"), entry);
            if (archetypeType == null)
            {
                Debug.LogWarning($"[balance-importer] weapon '{slug}': no archetype-config subclass for archetype='{entry.Value<string>("archetype")}' — leaving archetypeConfig=null");
                return;
            }

            Directory.CreateDirectory(ArchetypeOutputDir);
            var assetPath = $"{ArchetypeOutputDir}/Weapon_{slug}_archetype.asset";

            // Existing asset must match the dispatched type; if a previous import
            // landed on the wrong subclass (e.g. JSON archetype was edited), drop
            // it and re-create.
            var existing = AssetDatabase.LoadAssetAtPath<WeaponArchetypeConfig>(assetPath);
            if (existing != null && existing.GetType() != archetypeType)
            {
                AssetDatabase.DeleteAsset(assetPath);
                existing = null;
            }

            WeaponArchetypeConfig configSo;
            if (existing != null)
            {
                configSo = existing;
            }
            else
            {
                configSo = (WeaponArchetypeConfig)ScriptableObject.CreateInstance(archetypeType);
                AssetDatabase.CreateAsset(configSo, assetPath);
            }

            // Populate per-subclass fields from JSON (top-level baseline + per-level
            // perk carry-forward array — mirrors ImportWeapons' rate_ms / range carry-forward).
            using (var cfgSerialized = new SerializedObject(configSo))
            {
                if (configSo is MineArchetypeConfig)
                {
                    int armTime0 = entry.Value<int?>("arm_time_ms") ?? 0;
                    ApplyField(cfgSerialized, "armTimeMs", armTime0);
                    ApplyPerLevelInt(cfgSerialized, "armTimeMsPerLevel", entry["level_perks"] as JArray,
                                     "arm_time_ms", armTime0);
                }
                else if (configSo is CloudArchetypeConfig)
                {
                    int cloudLife0 = entry.Value<int?>("cloud_lifetime_ms") ?? 0;
                    int zaps0      = entry.Value<int?>("zaps_per_cloud")   ?? 0;
                    ApplyField(cfgSerialized, "cloudLifetimeMs", cloudLife0);
                    ApplyField(cfgSerialized, "zapsPerCloud",    zaps0);
                    ApplyPerLevelInt(cfgSerialized, "cloudLifetimeMsPerLevel",
                                     entry["level_perks"] as JArray, "cloud_lifetime_ms", cloudLife0);
                    ApplyPerLevelInt(cfgSerialized, "zapsPerCloudPerLevel",
                                     entry["level_perks"] as JArray, "zaps_per_cloud",    zaps0);
                }
                else if (configSo is SplashProjectileArchetypeConfig)
                {
                    float splash0 = entry.Value<float?>("splash_units_base") ?? 0f;
                    int   travel0 = entry.Value<int?>("travel_ms")           ?? 0;
                    ApplyField(cfgSerialized, "splashUnitsBase", splash0);
                    ApplyField(cfgSerialized, "travelMs",        travel0);
                    ApplyPerLevelFloat(cfgSerialized, "splashUnitsPerLevel",
                                       entry["level_perks"] as JArray, "splash_units", splash0);
                    ApplyPerLevelInt(cfgSerialized, "travelMsPerLevel",
                                     entry["level_perks"] as JArray, "travel_ms", travel0);
                }
                else if (configSo is AuraArchetypeConfig)
                {
                    float slow0 = entry.Value<float?>("slow_pct_base") ?? 0f;
                    int   tick0 = entry.Value<int?>("tick_lifetime_ms") ?? 0;
                    ApplyField(cfgSerialized, "slowPctBase",    slow0);
                    ApplyField(cfgSerialized, "tickLifetimeMs", tick0);
                    ApplyPerLevelFloat(cfgSerialized, "slowPctPerLevel",
                                       entry["level_perks"] as JArray, "slow_pct", slow0);
                    ApplyPerLevelInt(cfgSerialized, "tickLifetimeMsPerLevel",
                                     entry["level_perks"] as JArray, "tick_lifetime_ms", tick0);
                }
                else if (configSo is SummonArchetypeConfig)
                {
                    int life0 = entry.Value<int?>("lifetime_ms") ?? 0;
                    ApplyField(cfgSerialized, "lifetimeMs", life0);
                    ApplyPerLevelInt(cfgSerialized, "lifetimeMsPerLevel",
                                     entry["level_perks"] as JArray, "lifetime_ms", life0);
                }
                // Projectile / Beam subclasses are field-empty base cases — nothing to populate.

                cfgSerialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(configSo);
            }

            // Wire the sidecar onto WeaponDefinition.archetypeConfig. The caller
            // ApplyModifiedPropertiesWithoutUndo() will persist the reference.
            var prop = weaponSerialized.FindProperty("archetypeConfig");
            if (prop != null)
            {
                prop.objectReferenceValue = configSo;
            }
        }

        // Carries an int perk value forward across the 5 weapon levels, mirroring
        // the rate_ms / range carry-forward in ImportWeapons. Perk values at level
        // N apply to levels N..5; baseline applies to levels before any perk hits.
        private static void ApplyPerLevelInt(SerializedObject so, string arrayPath,
                                             JArray? perks, string perkName, int baseline)
        {
            var arrProp = so.FindProperty(arrayPath);
            if (arrProp == null || !arrProp.isArray) return;
            arrProp.arraySize = WeaponLevelCount;

            int current = baseline;
            for (int i = 0; i < WeaponLevelCount; i++)
            {
                int level1Based = i + 1;
                if (perks != null && level1Based >= 2)
                {
                    foreach (var p in perks.OfType<JObject>())
                    {
                        if ((p.Value<int?>("level") ?? -1) != level1Based) continue;
                        if (p.Value<string>("perk") != perkName) continue;
                        int? v = p.Value<int?>("value");
                        if (v.HasValue) current = v.Value;
                    }
                }
                arrProp.GetArrayElementAtIndex(i).intValue = current;
            }
        }

        private static void ApplyPerLevelFloat(SerializedObject so, string arrayPath,
                                               JArray? perks, string perkName, float baseline)
        {
            var arrProp = so.FindProperty(arrayPath);
            if (arrProp == null || !arrProp.isArray) return;
            arrProp.arraySize = WeaponLevelCount;

            float current = baseline;
            for (int i = 0; i < WeaponLevelCount; i++)
            {
                int level1Based = i + 1;
                if (perks != null && level1Based >= 2)
                {
                    foreach (var p in perks.OfType<JObject>())
                    {
                        if ((p.Value<int?>("level") ?? -1) != level1Based) continue;
                        if (p.Value<string>("perk") != perkName) continue;
                        float? v = p.Value<float?>("value");
                        if (v.HasValue) current = v.Value;
                    }
                }
                arrProp.GetArrayElementAtIndex(i).floatValue = current;
            }
        }

        private static void ImportPassives(ref int created, ref int updated)
        {
            var root = LoadJson("passives.json");
            if (root == null) return;
            var list = (root is JArray arr) ? (JArray)arr : (JArray)(root["passives"] ?? new JArray());
            foreach (JObject entry in list.OfType<JObject>())
            {
                var slug = entry.Value<string>("id") ?? entry.Value<string>("slug");
                if (string.IsNullOrEmpty(slug)) continue;

                var assetPath = $"{OutputDir}/Passive_{slug}.asset";
                var existed = File.Exists(assetPath);
                var so = LoadOrCreate<PassiveDefinition>(assetPath);

                using var serialized = new SerializedObject(so);
                ApplyField(serialized, "slug", slug);
                ApplyField(serialized, "displayName", entry.Value<string>("display_name"));
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(so);

                if (existed) updated++; else created++;
            }
        }

        private static void ImportEnemies(ref int created, ref int updated)
        {
            var root = LoadJson("enemies.json");
            if (root == null) return;
            var list = (root is JArray arr) ? (JArray)arr : (JArray)(root["enemies"] ?? new JArray());

            // File-level reference: player's baseline move speed (used to convert
            // enemy.scaling.speed_mult_vs_player → absolute units/sec for the SO).
            // Per docs/10-balance/00-formulas.md §3, base_move = 4.5 (Bunny anchor).
            // characters.json owns this constant; mirror the default here.
            const float playerBaseMoveUnitsPerSec = 4.5f;

            foreach (JObject entry in list.OfType<JObject>())
            {
                var slug = entry.Value<string>("id") ?? entry.Value<string>("slug");
                if (string.IsNullOrEmpty(slug)) continue;

                var assetPath = $"{OutputDir}/Enemy_{slug}.asset";
                var existed = File.Exists(assetPath);
                var so = LoadOrCreate<EnemyDefinition>(assetPath);

                using var serialized = new SerializedObject(so);
                ApplyField(serialized, "slug", slug);
                ApplyField(serialized, "displayName", entry.Value<string>("display_name"));
                // role is an enum on the SO (EnemyRole) but JSON ships a string ("swarmer"/"tank"/...).
                ApplyEnumByJsonString(serialized, "role", entry.Value<string>("role"));

                // --- Stats (minute-1 baseline) ---------------------------------------
                // Per docs/10-balance/00-formulas.md §9: enemies.json hp_base = minute-1 HP;
                // per-minute scaling (hp_per_min) is applied at runtime by the spawner —
                // NOT in the SO. The SO baseline is the minute-1 anchor only.
                // ADR-0006 values already in JSON (swarmer 6+4, elite 300+80, boss 2000/3000).
                var scaling = entry["scaling"] as JObject;
                if (scaling != null)
                {
                    // Boss uses hp_mid_boss/hp_end_boss instead of hp_base.
                    // For the SO baseline, pick hp_mid_boss (mid-run encounter) when present;
                    // the spawner picks mid- vs end- at run time per minute mark.
                    float baseHp =
                        scaling.Value<float?>("hp_base")
                        ?? scaling.Value<float?>("hp_mid_boss")
                        ?? scaling.Value<float?>("hp_end_boss")
                        ?? 0f;
                    ApplyField(serialized, "baseHP", baseHp);

                    float contactDmg = scaling.Value<float?>("contact_dmg") ?? 0f;
                    ApplyField(serialized, "contactDamage", contactDmg);

                    // ranged_dmg is optional (only ranged role + boss have it). Default 0.
                    float rangedDmg = scaling.Value<float?>("ranged_dmg") ?? 0f;
                    ApplyField(serialized, "rangedDamage", rangedDmg);

                    // Per §3 movement: enemy speed_mult_vs_player × player_base_move.
                    float speedMult = scaling.Value<float?>("speed_mult_vs_player") ?? 1f;
                    ApplyField(serialized, "moveSpeed", playerBaseMoveUnitsPerSec * speedMult);

                    // Clamped to [0, 0.75] per §11; EnemyDefinition.OnValidate enforces this.
                    float defMult = scaling.Value<float?>("defense_mult") ?? 0f;
                    ApplyField(serialized, "defenseMultiplier", defMult);
                }

                // Telegraph window (seconds) — tanks/elites/bosses telegraph their attacks.
                // JSON has multiple sources:
                //   - tank:  charge.telegraph_ms
                //   - ranged: ranged.telegraph_ms
                //   - elite/boss: telegraph_min_ms (top-level)
                int telegraphMs =
                    entry.Value<int?>("telegraph_min_ms")
                    ?? (entry["charge"] as JObject)?.Value<int?>("telegraph_ms")
                    ?? (entry["ranged"] as JObject)?.Value<int?>("telegraph_ms")
                    ?? 0;
                ApplyField(serialized, "telegraphWindowSeconds", telegraphMs / 1000f);

                // Note: DropTable fields populated by a separate drops.json importer pass
                // (deferred — ImportSingle("drops.json") is currently a no-op). Leaving at
                // struct defaults means no XP/gold/heart drops yet; this is a documented
                // follow-up wave, not a balance regression (the JSON still owns the numbers).
                // telegraphSfxKey is asset-curator territory; no JSON source.

                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(so);

                if (existed) updated++; else created++;
            }
        }

        // Single-instance JSONs (xp-curve, drops, economy, feel) — placeholder.
        // The destination SO type for each is project-specific (e.g., FeelDefinition).
        // For Phase 5 we only emit a stub asset that references the JSON content as
        // a string for runtime parsing; richer SO mirrors come in a later wave.
        private static void ImportSingle(string filename, string assetSuffix, ref int created, ref int updated)
        {
            var root = LoadJson(filename);
            if (root == null) return;
            // No-op for now — proper SO type-mapping lands in a later Editor wave.
            Debug.Log($"[balance-importer] {filename} parsed OK ({root.Type}); SO mirror deferred to Phase 5 follow-up");
        }

        private static void ApplyField(SerializedObject serialized, string fieldName, object? value)
        {
            if (value == null) return;
            var prop = serialized.FindProperty(fieldName);
            if (prop == null) return;
            switch (value)
            {
                case string s: prop.stringValue = s; break;
                case int i: prop.intValue = i; break;
                case float f: prop.floatValue = f; break;
                case double d: prop.floatValue = (float)d; break;
                case bool b: prop.boolValue = b; break;
            }
        }

        // Maps a JSON kebab-case enum string ("utility-beam", "swarmer") to the matching
        // SerializedProperty enum index. Match is case-insensitive on the alphanumeric
        // characters only (so "utility-beam" matches enum "Utility" prefix OR a full
        // "UtilityBeam" — first-match-wins by stripping non-letters).
        // If no enum member matches, the property is left at its previous value and a
        // warning is logged — callers can spot domain mismatches (e.g. boss role missing
        // from EnemyRole enum) without silently corrupting data.
        private static void ApplyEnumByJsonString(SerializedObject serialized, string fieldName, string? value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var prop = serialized.FindProperty(fieldName);
            if (prop == null || prop.propertyType != SerializedPropertyType.Enum) return;
            var names = prop.enumNames;
            string norm = NormalizeEnum(value);
            // First pass: exact normalized match ("projectile" → "Projectile").
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(NormalizeEnum(names[i]), norm, StringComparison.OrdinalIgnoreCase))
                {
                    prop.enumValueIndex = i;
                    return;
                }
            }
            // Second pass: first-kebab-token match. Lets JSON "utility-beam" find enum "Utility".
            string firstToken = value.Split('-')[0];
            string firstNorm = NormalizeEnum(firstToken);
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(NormalizeEnum(names[i]), firstNorm, StringComparison.OrdinalIgnoreCase))
                {
                    prop.enumValueIndex = i;
                    return;
                }
            }
            Debug.LogWarning($"[balance-importer] enum '{value}' has no match in {fieldName} enum (members: {string.Join(",", names)}) — left unchanged");
        }

        private static string NormalizeEnum(string s)
        {
            var chars = s.Where(char.IsLetterOrDigit).ToArray();
            return new string(chars);
        }
    }
}
#endif
