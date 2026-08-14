using UnityEngine;

public class PlayerDashState : IState
{
    private readonly PlayerController _player;
    private readonly StateMachine _stateMachine;
    private readonly PlayerMotor _motor;
    private readonly PlayerStatsConfig _config;
    private Vector3 _dashDirection;
    private float _elapsedTime;

    public PlayerDashState(PlayerController player, StateMachine stateMachine)
    {
        _player = player;
        _stateMachine = stateMachine;
        _motor = player.Motor;
        _config = player.Config;
    }

    public void Enter()
    {
        // Play dash animation, vfx dash
        _elapsedTime = 0f;
        _dashDirection = _player.transform.forward;
        _player.StartDashCooldown();
    }

    public void Update()
    {
        float remainingTime = _config.DashDuration - _elapsedTime;
        float frameTime = Mathf.Min(Time.deltaTime, remainingTime);
        float dashSpeed = _config.DashDistance / _config.DashDuration;
        float frameDistance = dashSpeed * frameTime;

        _motor.DashMove(_dashDirection, frameDistance);
        _elapsedTime += frameTime;

        if (_elapsedTime >= _config.DashDuration)
        {
            FinishDash();
        }
    }

    public void Exit()
    {
        
    }

    private void FinishDash()
    {
        if (_player.InputReader.MoveInput.sqrMagnitude > 0.01f)
        {
            _stateMachine.ChangeState(_player.MoveState);
        }
        else
        {
            _stateMachine.ChangeState(_player.IdleState);
        }
    }
}