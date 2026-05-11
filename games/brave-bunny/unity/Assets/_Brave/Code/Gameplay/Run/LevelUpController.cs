// Balance/00-formulas.md § 5: xp_to_next(level) = floor(20 × level^1.55 + 5).
using Brave.Gameplay.Events;
using UnityEngine;

namespace Brave.Gameplay.Run;

/// <summary>
/// XP accumulator. Raises <see cref="LevelUpChannel"/> when a threshold is crossed; the
/// draft UI listens, pauses Time.timeScale, and presents 3-card draft (UI lives in Brave.UI).
/// </summary>
public sealed class LevelUpController : MonoBehaviour
{
    [SerializeField] private LevelUpChannel _levelUpChannel;
    [SerializeField] private int _maxLevel = 30;

    private int _level = 1;
    private int _currentXp;

    public int Level => _level;
    public int CurrentXp => _currentXp;
    public int XpToNext => XpToNextForLevel(_level);

    public void AddXp(int amount)
    {
        if (amount <= 0 || _level >= _maxLevel) return;
        _currentXp += amount;
        while (_currentXp >= XpToNextForLevel(_level) && _level < _maxLevel)
        {
            _currentXp -= XpToNextForLevel(_level);
            _level++;
            _levelUpChannel?.Raise(new LevelUpEvent(_level, _currentXp));
        }
    }

    /// <summary>balance/00-formulas.md § 5: xp_to_next = floor(20 × level^1.55 + 5).</summary>
    public static int XpToNextForLevel(int level) =>
        Mathf.FloorToInt(20f * Mathf.Pow(level, 1.55f) + 5f);
}
