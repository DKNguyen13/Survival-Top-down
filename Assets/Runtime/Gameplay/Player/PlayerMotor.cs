using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    private CharacterController _characterController;
    private PlayerStatsConfig _config;
    private float _verticalVelocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public void Initialize(PlayerStatsConfig config) => _config = config;

    private void Update()
    {
        ApplyGravity();
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

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        _characterController.Move(Vector3.up * (_verticalVelocity * Time.deltaTime));
    }
}