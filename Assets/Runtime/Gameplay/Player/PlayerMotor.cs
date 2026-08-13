using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    [SerializeField] private PlayerStatsConfig _config;
    private CharacterController _characterController;
    public PlayerStatsConfig Config => _config;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        if (_config == null)
        {
            Debug.LogError($"{nameof(PlayerMotor)} missing PlayerStatsConfig.", this);
        }
    }

    public void Move(Vector3 direction)
    {
        if (direction.sqrMagnitude > 1f)
        {
            direction.Normalize();
        }

        _characterController.Move(direction * _config.MoveSpeed * Time.deltaTime);
    }

    public void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f) return;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _config.RotationSpeed * Time.deltaTime);
    }

    public void DashMove(Vector3 direction, float distance)
    {
        _characterController.Move(direction * distance);
    }
}