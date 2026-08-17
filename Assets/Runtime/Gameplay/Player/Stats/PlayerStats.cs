using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public float Armor { get; private set; }
    public float DamageMultiplier { get; private set; }
    public event Action<float, float> HealthChanged;
    public event Action Died;
    private float _nextPoisonTick;
    private float _poisonDamage;
    private int _poisonTicksRemaining;
    private static readonly Color PoisonColor = new(0.35f, 1f, 0.12f);

    private void Update()
    {
        if (!IsAlive || _poisonTicksRemaining <= 0 || Time.time < _nextPoisonTick) return;
        TakeDamage(_poisonDamage);

        // VFX
        PrototypeEffects.PlayPoisonHit(transform.position + Vector3.up, PoisonColor);
        _poisonTicksRemaining--;
        _nextPoisonTick += 1f;
    }

    #region Init data
    public void Initialize(PlayerStatsConfig config)
    {
        MaxHealth = config.MaxHealth;
        CurrentHealth = MaxHealth;
        Armor = config.Armor;
        DamageMultiplier = config.DamageMultiplier;
    }
    #endregion

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;
        float finalDamage = Mathf.Max(0f, damage - Armor);
        if (finalDamage <= 0f) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - finalDamage);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        
        Debug.Log($"Player took {finalDamage} damage. HP: {CurrentHealth}/{MaxHealth}", this);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    public void ApplyPoison(float damagePerTick, float duration)
    {
        if (!IsAlive || damagePerTick <= 0f || duration <= 0f) return;

        int totalTicks = Mathf.Max(1, Mathf.RoundToInt(duration));
        _poisonDamage = damagePerTick;
        TakeDamage(_poisonDamage);

        // VFX
        PrototypeEffects.PlayPoisonHit(transform.position + Vector3.up * 0.8f, PoisonColor);
        _poisonTicksRemaining = totalTicks - 1;
        _nextPoisonTick = Time.time + 1f;
    }

    #region Helper
    public bool IsAlive => CurrentHealth > 0f;

    public float GetOutgoingDamage(float baseDamage)
    {
        return baseDamage * (1f + DamageMultiplier);
    }

    public void IncreaseMaxHealth(float amount)
    {
        if (amount <= 0f) return;
        MaxHealth += amount;
        CurrentHealth += amount;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void AddArmor(float amount)
    {
        Armor += amount;
    }

    public void AddDamageMultiplier(float amount)
    {
        DamageMultiplier += amount;
    }
    #endregion

    private void Die()
    {
        Died?.Invoke();
        Debug.Log("Player died.", this);
    }
}