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
using Brave.Gameplay.Definitions;

namespace Brave.Boot.Editor
{
    public static class BalanceJsonImporter
    {
        // Relative paths from the Unity project root.
        private const string BalanceDataDir = "../data/balance";
        private const string OutputDir = "Assets/_Brave/Data/Balance";

        [MenuItem("Brave/Generate Balance SOs from JSON")]
        public static void GenerateAll()
        {
            Directory.CreateDirectory(OutputDir);

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
                ApplyField(serialized, "archetype", entry.Value<string>("archetype"));
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(so);

                if (existed) updated++; else created++;
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
                ApplyField(serialized, "role", entry.Value<string>("role"));
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
    }
}
#endif
