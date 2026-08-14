using UnityEngine;

public class MeleeEnemyAttack : EnemyAttack
{
    public override bool TryAttack(Transform target, IDamageable damageable)
    {
        if (target == null || damageable == null || !damageable.IsAlive) return false;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float rangeSqr = Config.AttackRange * Config.AttackRange;

        if (toTarget.sqrMagnitude > rangeSqr) return false;
        if (toTarget.sqrMagnitude <= .001f) return false;

        // Cone detection check
        Vector3 direction = toTarget.normalized;
        float angle = Vector3.Angle(transform.forward, direction);
        float halfAttackAngle = Config.AttackAngle * 0.5f;

        // Skip if target is outside the attack cone
        if (angle > halfAttackAngle) return false;

#if UNITY_EDITOR
        Debug.Log($"[MELEE HIT] time={Time.time:F3} " +
        $"damage={Config.AttackDamage} " +
        $"distance={Mathf.Sqrt(toTarget.sqrMagnitude):F2} " +
        $"angle={angle:F1}",
        this);
#endif

        damageable.TakeDamage(Config.AttackDamage);
        return true;
    }
}