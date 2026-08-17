using UnityEngine;

public class RangedEnemyAttack : EnemyAttack
{
    [SerializeField] private Transform _firePoint;

    public override bool TryAttack(Transform target, IDamageable damageable)
    {
        if (target == null || damageable == null || !damageable.IsAlive || ObjectPooling.Instance == null)
        {
            return false;
        }

        Vector3 direction = target.position - _firePoint.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > Config.AttackRange * Config.AttackRange)
        {
            return false;
        }

        // Get enemy bullet from pool
        GameObject enemyBulletGO = ObjectPooling.Instance.GetFromPool(PoolType.EnemyBullet, _firePoint.position, Quaternion.LookRotation(direction));
        EnemyProjectile enemyProjectile = enemyBulletGO.GetComponent<EnemyProjectile>();
        enemyProjectile.Setup(direction, Config);

        // VFX
        PrototypeEffects.PlayHit(_firePoint.position, new Color(0.35f, 1f, 0.12f));
        return true;
    }
}