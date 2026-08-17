using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private PlayerBomb _bombPrefab;
    [SerializeField] private LayerMask _enemyMask;

    private PlayerStatsConfig _config;
    private PlayerStats _stats;
    private PlayerInputReader _input;
    private float _nextShotTime;
    private float _nextChargeTime;
    private float _nextBombTime;
    private float _nextDashTime;
    private bool _subscribed;
    public int ShootCharges { get; private set; }
    public event Action<int, int> ShootChargesChanged;

    private void OnEnable()
    {
        SubscribeInput();
    }

    private void Start()
    {
        ShootChargesChanged?.Invoke(ShootCharges, MaxShootCharges);
    }

    private void Update()
    {
        RecoverShootCharge();
    }

    #region Init data
    public void Initialize(PlayerStatsConfig config, PlayerStats stats, PlayerInputReader input)
    {
        _config = config;
        _stats = stats;
        _input = input;
        ShootCharges = config.MaxCharges;
        _nextChargeTime = Time.time + config.ChargeRecovery;
        SubscribeInput();
    }
    #endregion

    public void TryShoot()
    {
        if (ShootCharges <= 0 || Time.time < _nextShotTime || ObjectPooling.Instance == null) return;

        SpawnProjectile(-_config.SpreadAngle);
        SpawnProjectile(0f);
        SpawnProjectile(_config.SpreadAngle);
        PrototypeEffects.PlayHit(_firePoint.position, new Color(0.1f, 0.9f, 1f));

        ShootCharges--;
        _nextShotTime = Time.time + _config.ShotInterval;
        if (ShootCharges == MaxShootCharges - 1)
        {
            _nextChargeTime = Time.time + _config.ChargeRecovery;
        }
        ShootChargesChanged?.Invoke(ShootCharges, MaxShootCharges);
    }

    public void TryBomb()
    {
        if (Time.time < _nextBombTime || ObjectPooling.Instance == null) return;

        GameObject bombObject = ObjectPooling.Instance.GetFromPool(PoolType.Bomb, transform.position, Quaternion.identity);
        PlayerBomb bomb = bombObject.GetComponent<PlayerBomb>();
        bomb.Setup(_stats.GetOutgoingDamage(_config.BombDamage), _config.BombDelay, _config.BombRadius, _enemyMask);
        _nextBombTime = Time.time + _config.BombCooldown;
    }

    public bool TryStartDash()
    {
        if (Time.time < _nextDashTime) return false;
        _nextDashTime = Time.time + _config.DashCooldown;
        return true;
    }

    public void CompleteDash(Vector3 position)
    {
        DamageArea(position, _config.DashExplosionRadius, _stats.GetOutgoingDamage(_config.DashDamage), _enemyMask);
        PrototypeEffects.PlayBomb(position, _config.DashExplosionRadius, new Color(0.2f, 0.85f, 1f));
    }

    private void RecoverShootCharge()
    {
        if (ShootCharges >= MaxShootCharges || Time.time < _nextChargeTime) return;
        ShootCharges++;
        _nextChargeTime += _config.ChargeRecovery;
        ShootChargesChanged?.Invoke(ShootCharges, MaxShootCharges);
    }

    private void SpawnProjectile(float angle)
    {
        Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
        GameObject projectileObject = ObjectPooling.Instance.GetFromPool(PoolType.PlayerBullet, _firePoint.position, Quaternion.LookRotation(direction));
        projectileObject.GetComponent<PlayerProjectile>().Setup(transform, direction, _stats.GetOutgoingDamage(_config.ShotDamage), _config.ProjectileSpeed, _config.ProjectileRange);
    }

    public static void DamageArea(Vector3 center, float radius, float damage, LayerMask mask)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius, mask, QueryTriggerInteraction.Ignore);
        HashSet<EnemyHealth> damaged = new HashSet<EnemyHealth>();

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth target = hits[i].GetComponentInParent<EnemyHealth>();
            if (target != null && target.IsAlive && damaged.Add(target))
            {
                target.TakeDamage(damage);
            }
        }
    }

    private static float Remaining(float readyTime) => Mathf.Max(0f, readyTime - Time.time);

    private static float Cooldown01(float readyTime, float duration)
    {
        return duration <= 0f ? 0f : Mathf.Clamp01(Remaining(readyTime) / duration);
    }

    private void SubscribeInput()
    {
        if (_subscribed || _input == null || !isActiveAndEnabled) return;
        _input.ShootPressed += TryShoot;
        _input.BombPressed += TryBomb;
        _subscribed = true;
    }

    private void OnDisable()
    {
        if (_input == null) return;
        _input.ShootPressed -= TryShoot;
        _input.BombPressed -= TryBomb;
        _subscribed = false;
    }

    // Getter, Setter
    public int MaxShootCharges => _config.MaxCharges;
    public float ShootChargeCooldown01 => ShootCharges >= MaxShootCharges ? 0f : Cooldown01(_nextChargeTime, _config.ChargeRecovery);
    public float BombCooldown01 => Cooldown01(_nextBombTime, _config.BombCooldown);
    public float DashCooldown01 => Cooldown01(_nextDashTime, _config.DashCooldown);
    public float BombCooldownRemaining => Remaining(_nextBombTime);
    public float DashCooldownRemaining => Remaining(_nextDashTime);
}