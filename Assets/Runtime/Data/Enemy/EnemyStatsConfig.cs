using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatsConfig", menuName = "Survival/Data/Enemy Stats Config")]
public class EnemyStatsConfig : ScriptableObject
{
    [Header("Stats")]
    [SerializeField, Min(1f)] private float _maxHealth = 220f;
    [SerializeField, Min(0f)] private float _moveSpeed = 3f;

    [Header("Combat")]
    [SerializeField, Min(0f)] private float _attackRange = 1.3f;
    [SerializeField, Min(0f)] private float _attackDamage = 30f;
    [SerializeField, Range(0f, 360f)] private float _attackAngle = 50f;
    [SerializeField, Min(0f)] private float _recoveryDuration = 1f;

    [Header("Reward")]
    [SerializeField, Min(0)] private int _experienceReward = 30;

    public float MaxHealth => _maxHealth;
    public float MoveSpeed => _moveSpeed;

    public float AttackRange => _attackRange;
    public float AttackDamage => _attackDamage;
    public float AttackAngle => _attackAngle;
    public float RecoveryDuration => _recoveryDuration;
    public int ExperienceReward => _experienceReward;
}