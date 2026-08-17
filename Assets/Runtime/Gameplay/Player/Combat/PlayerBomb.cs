using UnityEngine;

public class PlayerBomb : MonoBehaviour
{
    private float _damage;
    private float _radius;
    private float _explodeAt;
    private LayerMask _mask;

    private void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * 14f) * 0.08f;
        transform.localScale = Vector3.one * pulse;
        
        if (Time.time < _explodeAt) return;
        PlayerSkills.DamageArea(transform.position, _radius, _damage, _mask);

        // VFX
        PrototypeEffects.PlayBomb(transform.position, _radius, new Color(1f, 0.35f, 0.08f));
        CameraShake.Play(transform.position, 0.7f);

        // Return to pool
        ObjectPooling.Instance.ReturnToPool(PoolType.Bomb, gameObject);
    }

    #region Setup
    public void Setup(float damage, float delay, float radius, LayerMask mask)
    {
        _damage = damage;
        _radius = radius;
        _mask = mask;
        _explodeAt = Time.time + delay;
    }
    #endregion
}