// QA — SaveData model EditMode tests
// Subject under test: BraveBunny.Systems.Save.SaveData POCO + nested sections.
// ADR-0008: every persisted field must carry [JsonProperty] for rename-safe forward-compat.
// Spec: docs/06-tech-spec/03-save-system.md § Save payload schema.

using System.Linq;
using System.Reflection;
using Brave.Systems.Save;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Systems
{
    [TestFixture]
    public class SaveModelTests
    {
        private const int ExpectedFreshVersion = 1;          // SaveHeader.CurrentVersion
        private const string ExpectedDefaultDisplayName = "Player";
        private const string ExpectedDefaultLanguage = "en";

        [Test]
        public void SaveModel_DefaultsHaveNoNullCollections()
        {
            var data = new SaveData();
            Assert.That(data.Characters, Is.Not.Null, "Characters dict must initialize empty, not null");
            Assert.That(data.Weapons, Is.Not.Null);
            Assert.That(data.Passives, Is.Not.Null);
            Assert.That(data.Cosmetics, Is.Not.Null);
            Assert.That(data.Achievements, Is.Not.Null);
            Assert.That(data.BattlePass, Is.Not.Null);
            Assert.That(data.BattlePass.ClaimedFreeTiers, Is.Not.Null);
            Assert.That(data.BattlePass.ClaimedPremiumTiers, Is.Not.Null);
            Assert.That(data.DailyMissions, Is.Not.Null);
            Assert.That(data.DailyMissions.Missions, Is.Not.Null);
            Assert.That(data.DailyStreak, Is.Not.Null);
            Assert.That(data.Settings, Is.Not.Null);
            Assert.That(data.Stats, Is.Not.Null);
            Assert.That(data.Player, Is.Not.Null);
            Assert.That(data.Currencies, Is.Not.Null);
        }

        [Test]
        public void SaveModel_VersionIsSetOnNew()
        {
            var data = new SaveData();
            Assert.That(data.Version, Is.EqualTo(SaveHeader.CurrentVersion));
            Assert.That(data.Version, Is.EqualTo(ExpectedFreshVersion));
        }

        [Test]
        public void SaveModel_DefaultsMatchSpec()
        {
            var data = new SaveData();
            Assert.That(data.Player.DisplayName, Is.EqualTo(ExpectedDefaultDisplayName));
            Assert.That(data.Player.Language, Is.EqualTo(ExpectedDefaultLanguage));
        }

        /// <summary>
        /// Per ADR-0008: SaveData (and every nested OptIn class) must declare
        /// <see cref="JsonObjectAttribute"/> with MemberSerialization.OptIn AND
        /// every public instance field must carry a <see cref="JsonPropertyAttribute"/>.
        /// </summary>
        [Test]
        public void SaveModel_AllFieldsOptInSerialized()
        {
            var rootType = typeof(SaveData);
            AssertOptInAndAnnotated(rootType);

            // walk all nested classes that are persisted
            foreach (var nested in rootType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (nested.IsClass)
                    AssertOptInAndAnnotated(nested);
            }
        }

        private static void AssertOptInAndAnnotated(System.Type type)
        {
            var objAttr = type.GetCustomAttribute<JsonObjectAttribute>();
            Assert.That(objAttr, Is.Not.Null, $"{type.FullName} must declare [JsonObject(MemberSerialization.OptIn)]");
            Assert.That(objAttr.MemberSerialization, Is.EqualTo(MemberSerialization.OptIn),
                $"{type.FullName} must be MemberSerialization.OptIn (ADR-0008)");

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var f in fields.Where(f => !f.IsStatic))
            {
                var prop = f.GetCustomAttribute<JsonPropertyAttribute>();
                Assert.That(prop, Is.Not.Null,
                    $"{type.FullName}.{f.Name} must carry [JsonProperty] (ADR-0008 rename-safe contract)");
            }
        }
    }
}
