public class EnemyAttackState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private bool _hasAttacked;

    public EnemyAttackState(EnemyController enemy, StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
    }

    public void Enter()
    {
        _hasAttacked = false;
    }

    public void Update()
    {
        if (_enemy.Target == null || _enemy.TargetDamageable == null || !_enemy.TargetDamageable.IsAlive)
        {
            _stateMachine.ChangeState(_enemy.ChaseState);
            return;
        }

        if (!_hasAttacked)
        {
            _enemy.Motor.FaceTarget(_enemy.Target.position);
            bool attacked = _enemy.Attack.TryAttack(_enemy.Target, _enemy.TargetDamageable);
            _hasAttacked = true;

            if (!attacked)
            {
                _stateMachine.ChangeState(_enemy.ChaseState);
            }
            return;
        }

        _stateMachine.ChangeState(_enemy.RecoveryState);
    }

    public void Exit()
    {
        
    }
}