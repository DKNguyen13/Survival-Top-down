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

    [Header("Dash")]
    [SerializeField] private float _dashDistance = 3f;
    [SerializeField] private float _dashDuration = 0.5f;
    [SerializeField] private float _dashCooldown = 6f;

    public float MaxHealth => _maxHealth;
    public float Armor => _armor;
    public float DamageMultiplier => _damageMultiplier;

    public float MoveSpeed => _moveSpeed;
    public float RotationSpeed => _rotationSpeed;

    public float DashDistance => _dashDistance;
    public float DashDuration => _dashDuration;
    public float DashCooldown => _dashCooldown;
}