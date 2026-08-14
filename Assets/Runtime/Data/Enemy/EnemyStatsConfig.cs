using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatsConfig", menuName = "Survival/Data/Enemy Stats Config")]
public class EnemyStatsConfig : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private float _maxHealth = 220f;
    [SerializeField] private float _moveSpeed = 3f;

    [Header("Combat")]
    [SerializeField] private float _attackRange = 1.3f;
    [SerializeField] private float _recoveryDuration = 1f;

    public float MaxHealth => _maxHealth;
    public float MoveSpeed => _moveSpeed;
    public float AttackRange => _attackRange;
    public float RecoveryDuration => _recoveryDuration;
}