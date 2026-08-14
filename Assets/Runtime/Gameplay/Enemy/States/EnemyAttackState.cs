public class EnemyAttackState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;

    public EnemyAttackState(EnemyController enemy, StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
    }

    public void Enter()
    {
        
    }

    public void Update()
    {
        if (_enemy.Target == null || _enemy.TargetDamageable == null || !_enemy.TargetDamageable.IsAlive)
        {
            _stateMachine.ChangeState(_enemy.ChaseState);
            return;
        }

        // Chase State locked the attack direction when it entered this state.
        // The player can avoid the hit by leaving the range or cone before this frame.
        if (_enemy.Attack.TryAttack(_enemy.Target, _enemy.TargetDamageable))
        {
            _stateMachine.ChangeState(_enemy.RecoveryState);
        }
        else
        {
            _stateMachine.ChangeState(_enemy.ChaseState);
        }
    }

    public void Exit()
    {
        
    }
}