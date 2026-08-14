using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatsConfig", menuName = "Survival/Data/Enemy Stats Config")]
public class EnemyStatsConfig : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private float _maxHealth = 220f;
    [SerializeField] private float _moveSpeed = 3f;

    [Header("Combat")]
    [SerializeField] private float _attackRange = 1.3f;
    [SerializeField] private float _attackDamage = 30f;
    [SerializeField] private float _attackAngle = 50f;
    [SerializeField] private float _recoveryDuration = 1f;

    [Header("Reward")]
    [SerializeField] private int _experienceReward = 30;

    public float MaxHealth => _maxHealth;
    public float MoveSpeed => _moveSpeed;

    public float AttackRange => _attackRange;
    public float AttackDamage => _attackDamage;
    public float AttackAngle => _attackAngle;
    public float RecoveryDuration => _recoveryDuration;

    public int ExperienceReward => _experienceReward;
}