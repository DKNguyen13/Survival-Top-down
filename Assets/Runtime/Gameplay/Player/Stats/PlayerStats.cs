using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public float Armor { get; private set; }
    public float DamageMultiplier { get; private set; }

    public bool IsAlive => CurrentHealth > 0f;
    public event Action<float, float> HealthChanged;
    public event Action Died;

    public void Initialize(PlayerStatsConfig config)
    {
        MaxHealth = config.MaxHealth;
        CurrentHealth = MaxHealth;
        Armor = config.Armor;
        DamageMultiplier = config.DamageMultiplier;
    }

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

    private void Die()
    {
        Died?.Invoke();
        Debug.Log("Player died.", this);
    }
}