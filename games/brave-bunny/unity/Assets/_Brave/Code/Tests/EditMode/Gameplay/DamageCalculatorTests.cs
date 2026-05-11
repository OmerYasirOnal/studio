// QA — DamageCalculator EditMode tests
// Subject under test: BraveBunny.Gameplay.Damage.DamageFormula
// Specs: docs/10-balance/00-formulas.md §1 Damage, §2 Crit roll, §11 Defense clamp
// User stories: US-19 (auto-attack visibility), US-23 (current → next deltas) — depend on stable damage math.
// Performance target: iPhone 12 baseline (see docs/06-tech-spec/05-performance-budget.md);
//                     formula is pure-CPU and runs on every contact event.

using Brave.Gameplay.Damage;
using Brave.Gameplay.Definitions;
using NUnit.Framework;

namespace Brave.Tests.EditMode.Gameplay
{
    [TestFixture]
    public class DamageCalculatorTests
    {
        // ---- constants (no magic numbers per CLAUDE.md principle 6) ----
        private const float WeaponBaseDamage = 10f;
        private const float WeaponFireRate = 0.5f;
        private const float WeaponRange = 4f;
        private const int WeaponProjectiles = 1;
        private const float HeroBaseHp = 100f;
        private const float HeroMoveSpeed = 4.5f;
        private const float HeroMagnet = 1f;
        private const float HeroBaseCharMult = 1f;
        private const float HeroNoCrit = 0f;
        private const float HeroCritDamageX2 = 1f;       // +1.0 → 2x multiplier
        private const float HeroFullCrit = 1f;           // 100% crit (clamped to CritRateMax internally)
        private const float CritRollAlwaysHits = 0f;     // critRoll < critRate → guaranteed crit
        private const float CritRollNeverHits = 0.999f;
        private const float NoDefense = 0f;
        private const float OverCapDefense = 0.9f;       // clamped to DefenseMultMax 0.75
        private const float ExpectedFloor = 1f;

        private static CharacterStats BaselineHero(float damageMult = HeroBaseCharMult, float critRate = HeroNoCrit, float critDamage = HeroCritDamageX2)
        {
            return new CharacterStats
            {
                baseHP = HeroBaseHp,
                baseMoveSpeed = HeroMoveSpeed,
                damageMultiplier = damageMult,
                critRate = critRate,
                critDamage = critDamage,
                magnetMultiplier = HeroMagnet,
                xpGemValueBonus = 0f,
            };
        }

        private static WeaponLevelData BaselineWeapon(float damage = WeaponBaseDamage)
        {
            return new WeaponLevelData
            {
                damage = damage,
                fireRate = WeaponFireRate,
                range = WeaponRange,
                projectiles = WeaponProjectiles,
                upgradeFlavor = "test",
            };
        }

        [Test]
        public void Damage_Base_NoMods_ReturnsBaseDamage()
        {
            // arrange
            var weapon = BaselineWeapon();
            var hero = BaselineHero();
            // act
            var dmg = DamageFormula.Compute(weapon, hero, NoDefense, CritRollNeverHits, out var isCrit);
            // assert
            Assert.That(dmg, Is.EqualTo(WeaponBaseDamage).Within(0.0001f));
            Assert.That(isCrit, Is.False);
        }

        [TestCase(0.5f, 5f)]
        [TestCase(1.0f, 10f)]
        [TestCase(1.5f, 15f)]
        [TestCase(2.0f, 20f)]
        public void Damage_AppliesCharacterMult(float charMult, float expected)
        {
            var dmg = DamageFormula.Compute(
                BaselineWeapon(), BaselineHero(damageMult: charMult), NoDefense, CritRollNeverHits, out _);
            Assert.That(dmg, Is.EqualTo(expected).Within(0.0001f));
        }

        [TestCase(5f)]
        [TestCase(10f)]
        [TestCase(25f)]
        public void Damage_AppliesWeaponLevelMult(float weaponDamage)
        {
            // weapon-level scaling is pre-baked into weapon.damage per DamageFormula doc comment.
            var dmg = DamageFormula.Compute(
                BaselineWeapon(damage: weaponDamage), BaselineHero(), NoDefense, CritRollNeverHits, out _);
            Assert.That(dmg, Is.EqualTo(weaponDamage).Within(0.0001f));
        }

        [Test]
        public void Damage_CritIncreasesByCritMult()
        {
            var weapon = BaselineWeapon();
            var hero = BaselineHero(critRate: HeroFullCrit, critDamage: HeroCritDamageX2);
            var dmg = DamageFormula.Compute(weapon, hero, NoDefense, CritRollAlwaysHits, out var isCrit);
            // crit doubles damage at +1.0 critDamage (1 + 1.0 = 2x)
            Assert.That(isCrit, Is.True);
            Assert.That(dmg, Is.EqualTo(WeaponBaseDamage * 2f).Within(0.0001f));
        }

        [Test]
        public void Damage_DefenseClampsAtSeventyFivePercent()
        {
            // Even with 90% defense input, formula clamps to 75% per balance §11.
            var dmg = DamageFormula.Compute(
                BaselineWeapon(), BaselineHero(), OverCapDefense, CritRollNeverHits, out _);
            var expected = WeaponBaseDamage * (1f - DamageFormula.DefenseMultMax);
            Assert.That(dmg, Is.EqualTo(expected).Within(0.0001f));
        }

        [TestCase(0.001f)]
        [TestCase(0f)]
        public void Damage_NeverReturnsZero_MinimumOne(float weaponDamage)
        {
            var dmg = DamageFormula.Compute(
                BaselineWeapon(damage: weaponDamage), BaselineHero(), NoDefense, CritRollNeverHits, out _);
            Assert.That(dmg, Is.GreaterThanOrEqualTo(ExpectedFloor));
            Assert.That(dmg, Is.EqualTo(DamageFormula.DamageFloor).Within(0.0001f));
        }

        [Test]
        public void Damage_CritRate_ClampsAt95Percent()
        {
            // hero.critRate of 1.0 should clamp to CritRateMax = 0.95.
            // critRoll just below 0.95 must hit; just above 0.95 must miss.
            var weapon = BaselineWeapon();
            var hero = BaselineHero(critRate: HeroFullCrit, critDamage: HeroCritDamageX2);

            DamageFormula.Compute(weapon, hero, NoDefense, DamageFormula.CritRateMax - 0.0001f, out var critsBelow);
            DamageFormula.Compute(weapon, hero, NoDefense, DamageFormula.CritRateMax + 0.0001f, out var critsAbove);

            Assert.That(critsBelow, Is.True, "roll below CritRateMax should crit");
            Assert.That(critsAbove, Is.False, "roll above CritRateMax must not crit (95% cap)");
        }
    }
}
