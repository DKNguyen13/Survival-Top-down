using UnityEngine;

public class EnemyMotor : MonoBehaviour
{
    private EnemyStatsConfig _config;

    public void Initialize(EnemyStatsConfig config) => _config = config;

    public void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= .001f) return;
        direction.Normalize();
        transform.position += direction * _config.MoveSpeed * Time.deltaTime;
        FaceDirection(direction);
    }

    public void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;
        FaceDirection(direction);
    }

    private void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= .001f) return;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}