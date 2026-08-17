public class PlayerIdleState : IState
{
    private readonly PlayerController _player;
    private readonly StateMachine _stateMachine;

    public PlayerIdleState(PlayerController player, StateMachine stateMachine)
    {
        _player = player;
        _stateMachine = stateMachine;
    }

    public void Enter()
    {
        // Play idle animation
    }

    public void Update()
    {
        if(_player.InputReader.MoveInput.sqrMagnitude > .01f)
        {
            _stateMachine.ChangeState(_player.MoveState);
        }
    }

    public void Exit()
    {
        
    }
}