using UnityEngine;

public abstract class EnemyAttack : MonoBehaviour
{
    protected EnemyStatsConfig Config { get; private set; }

    public void Initialize(EnemyStatsConfig config) => Config = config;

    public abstract bool TryAttack(Transform target, IDamageable damageable);
}