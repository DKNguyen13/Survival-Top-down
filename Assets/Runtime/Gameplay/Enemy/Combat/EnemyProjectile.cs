using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 _direction;
    private float _speed;
    private float _remainingRange;
    private float _poisonDamage;
    private float _poisonDuration;

    private void Update()
    {
        float distance = Mathf.Min(_speed * Time.deltaTime, _remainingRange);
        transform.position += _direction * distance;
        _remainingRange -= distance;

        if (_remainingRange <= 0f) {
            ObjectPooling.Instance.ReturnToPool(PoolType.EnemyBullet, gameObject);
        }
    }

    #region Setup
    public void Setup(Vector3 direction, EnemyStatsConfig config)
    {
        _direction = direction.normalized;
        _speed = config.ProjectileSpeed;
        _remainingRange = config.ProjectileRange;
        _poisonDamage = config.PoisonDamagePerTick;
        _poisonDuration = config.PoisonDuration;
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null) return;
        player.ApplyPoison(_poisonDamage, _poisonDuration);
        
        // VFX and audio
        PrototypeEffects.PlayHit(transform.position, new Color(0.45f, 1f, 0.2f));
        AudioManager.PlaySfx(SfxId.PoisonHit, transform.position);
        
        // Return to pool
        ObjectPooling.Instance.ReturnToPool(PoolType.EnemyBullet, gameObject);
    }
}