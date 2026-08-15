using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "Survival/Data/Player Stats Config")]
public class PlayerStatsConfig : ScriptableObject
{
    [Header("Base Stats")]
    [SerializeField] private float _maxHealth = 500f;
    [SerializeField] private float _armor = 0f;
    [SerializeField] private float _damageMultiplier = 0f;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _rotationSpeed = 180f;

    [Header("Normal Attack")]
    [SerializeField] private float _shotDamage = 10f;
    [SerializeField] private float _shotInterval = 0.5f;
    [SerializeField] private float _chargeRecovery = 3f;
    [SerializeField] private int _maxCharges = 3;
    [SerializeField] private float _spreadAngle = 15f;
    [SerializeField] private float _projectileSpeed = 10f;
    [SerializeField] private float _projectileRange = 10f;

    [Header("Bomb")]
    [SerializeField] private float _bombDamage = 50f;
    [SerializeField] private float _bombDelay = 2f;
    [SerializeField] private float _bombRadius = 5f;
    [SerializeField] private float _bombCooldown = 12f;

    [Header("Dash")]
    [SerializeField] private float _dashDistance = 3f;
    [SerializeField] private float _dashDuration = 0.5f;
    [SerializeField] private float _dashCooldown = 6f;
    [SerializeField] private float _dashDamage = 15f;
    [SerializeField] private float _dashExplosionRadius = 3f;

    [Header("Progression")]
    [SerializeField] private int _experienceToLevel = 100;
    [SerializeField] private float _healthPerLevel = 40f;
    [SerializeField] private float _armorPerLevel = 2f;
    [SerializeField] private float _damageMultiplierPerLevel = 0.1f;

    // Getter, Setter 
    public float MaxHealth => _maxHealth;
    public float Armor => _armor;
    public float DamageMultiplier => _damageMultiplier;
    public float MoveSpeed => _moveSpeed;
    public float RotationSpeed => _rotationSpeed;
    public float ShotDamage => _shotDamage;
    public float ShotInterval => _shotInterval;
    public float ChargeRecovery => _chargeRecovery;
    public int MaxCharges => _maxCharges;
    public float SpreadAngle => _spreadAngle;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileRange => _projectileRange;
    public float BombDamage => _bombDamage;
    public float BombDelay => _bombDelay;
    public float BombRadius => _bombRadius;
    public float BombCooldown => _bombCooldown;
    public float DashDistance => _dashDistance;
    public float DashDuration => _dashDuration;
    public float DashCooldown => _dashCooldown;
    public float DashDamage => _dashDamage;
    public float DashExplosionRadius => _dashExplosionRadius;
    public int ExperienceToLevel => _experienceToLevel;
    public float HealthPerLevel => _healthPerLevel;
    public float ArmorPerLevel => _armorPerLevel;
    public float DamageMultiplierPerLevel => _damageMultiplierPerLevel;
}