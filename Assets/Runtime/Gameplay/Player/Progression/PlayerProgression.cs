using System;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    private PlayerStats _stats;
    private PlayerStatsConfig _config;

    public int Level { get; private set; } = 1;
    public int CurrentExp { get; private set; }
    public event Action<int> LevelChanged;
    public event Action<int, int> ExpChanged;

    public void Initialize(PlayerStatsConfig config, PlayerStats stats)
    {
        _config = config;
        _stats = stats;
        Level = 1;
        CurrentExp = 0;
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        CurrentExp += amount;

        while (CurrentExp >= ExpToLevelUp)
        {
            CurrentExp -= ExpToLevelUp;
            LevelUp();
        }

        ExpChanged?.Invoke(CurrentExp, ExpToLevelUp);
    }

    private void LevelUp()
    {
        Level++;
        _stats.IncreaseMaxHealth(_config.HealthPerLevel);
        _stats.AddArmor(_config.ArmorPerLevel);
        _stats.AddDamageMultiplier(_config.DamageMultiplierPerLevel);

        // VFX
        PrototypeEffects.PlayLevelUp(transform.position, new Color(1f, 0.85f, 0.15f));
        
        LevelChanged?.Invoke(Level);
    }

    // Getter, Setter
    public int ExpToLevelUp => _config.ExperienceToLevel;
}