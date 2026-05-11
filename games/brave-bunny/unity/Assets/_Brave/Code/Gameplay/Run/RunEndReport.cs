namespace Brave.Gameplay.Run;

/// <summary>Per-run totals banked at RunEnd. Pass-by-value (readonly struct) — zero GC.</summary>
public readonly struct RunEndReport
{
    public readonly RunResult result;
    public readonly float runSeconds;
    public readonly int enemiesKilled;
    public readonly int elitesKilled;
    public readonly int bossesKilled;
    public readonly int carrotsEarned;
    public readonly int soulShardsEarned;
    public readonly int passXpEarned;
    public readonly int finalLevel;

    public RunEndReport(
        RunResult result, float runSeconds,
        int enemiesKilled, int elitesKilled, int bossesKilled,
        int carrotsEarned, int soulShardsEarned, int passXpEarned, int finalLevel)
    {
        this.result = result;
        this.runSeconds = runSeconds;
        this.enemiesKilled = enemiesKilled;
        this.elitesKilled = elitesKilled;
        this.bossesKilled = bossesKilled;
        this.carrotsEarned = carrotsEarned;
        this.soulShardsEarned = soulShardsEarned;
        this.passXpEarned = passXpEarned;
        this.finalLevel = finalLevel;
    }
}
