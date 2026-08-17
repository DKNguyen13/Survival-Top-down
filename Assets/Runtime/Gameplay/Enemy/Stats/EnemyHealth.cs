using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    private EnemyStatsConfig _config;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => _config != null ? _config.MaxHealth : 0f;
    public bool IsAlive => CurrentHealth > 0f;

    public event Action<float, float> HealthChanged;
    public event Action<EnemyHealth> Died;

    public void Initialize(EnemyStatsConfig config)
    {
        _config = config;
        ResetHealth();
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;
        if (damage <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        //AudioManager.PlaySfx(SfxId.EnemyDeath, transform.position);
        Died?.Invoke(this);
    }
}