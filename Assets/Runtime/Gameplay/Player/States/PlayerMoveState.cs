using UnityEngine;

public class PlayerMoveState : IState
{
    private readonly PlayerController _player;
    private readonly StateMachine _stateMachine;

    public PlayerMoveState(PlayerController player, StateMachine stateMachine)
    {
        _player = player;
        _stateMachine = stateMachine;
    }

    public void Enter()
    {
        //Play run animation
    }

    public void Update()
    {
        Vector2 input = _player.InputReader.MoveInput;
        if (input.sqrMagnitude <= .01f)
        {
            _stateMachine.ChangeState(_player.IdleState);
            return;
        }

        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);
        _player.Motor.Move(moveDirection);
        _player.Motor.RotateTowards(moveDirection);
    }

    public void Exit()
    {
        
    }
}