using System;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    [SerializeField] private PlayerStats _stats;

    [Header("Progression")]
    [SerializeField] private int _expToLevelUp = 100;
    [SerializeField] private float _healthPerLevel = 40f;
    [SerializeField] private float _armorPerLevel = 2f;
    [SerializeField] private float _damageMultiplierPerLevel = 0.1f;

    public int Level { get; private set; } = 1;
    public int CurrentExp { get; private set; }

    public int ExpToLevelUp => _expToLevelUp;

    public event Action<int> LevelChanged;
    public event Action<int, int> ExpChanged;

    private void Awake()
    {
        if (_stats == null)
        {
            _stats = GetComponent<PlayerStats>();
        }
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        CurrentExp += amount;

        while (CurrentExp >= _expToLevelUp)
        {
            CurrentExp -= _expToLevelUp;
            LevelUp();
        }

        ExpChanged?.Invoke(CurrentExp, _expToLevelUp);
    }

    private void LevelUp()
    {
        Level++;
        _stats.IncreaseMaxHealth(_healthPerLevel);
        _stats.AddArmor(_armorPerLevel);
        _stats.AddDamageMultiplier(_damageMultiplierPerLevel);
        LevelChanged?.Invoke(Level);
    }
}