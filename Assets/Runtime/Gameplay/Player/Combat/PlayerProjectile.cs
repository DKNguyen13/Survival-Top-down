using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    private Transform _owner;
    private Vector3 _direction;
    private float _speed;
    private float _remainingRange;
    private float _damage;

    private void Update()
    {
        float distance = Mathf.Min(_speed * Time.deltaTime, _remainingRange);
        transform.position += _direction * distance;
        _remainingRange -= distance;

        if (_remainingRange <= 0f)
        {
            ObjectPooling.Instance.ReturnToPool(PoolType.PlayerBullet, gameObject);
        }
    }

    #region Setup
    public void Setup(Transform owner, Vector3 direction, float damage, float speed, float range)
    {
        _owner = owner;
        _direction = direction.normalized;
        _damage = damage;
        _speed = speed;
        _remainingRange = range;
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (_owner != null && other.transform.IsChildOf(_owner)) return;
        if (other.GetComponentInParent<PlayerProjectile>() != null) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && damageable.IsAlive) damageable.TakeDamage(_damage);

        // VFX
        PrototypeEffects.PlayHit(transform.position, new Color(1f, 0.8f, 0.2f));
        AudioManager.PlaySfx(SfxId.PlayerBulletHit, transform.position);

        // Return to pool
        ObjectPooling.Instance.ReturnToPool(PoolType.PlayerBullet, gameObject);
    }
}