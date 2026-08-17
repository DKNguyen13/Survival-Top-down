using UnityEngine;

public class EnemyChaseState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private readonly EnemyMotor _motor;
    private readonly EnemyStatsConfig _config;

    public EnemyChaseState(EnemyController enemy, StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
        _motor = enemy.Motor;
        _config = enemy.Config;
    }

    public void Enter()
    {
        
    }

    public void Update()
    {
        if (_enemy.Target == null) return;

        Vector3 difference = _enemy.Target.position - _enemy.transform.position;
        difference.y = 0f;
        float attackRangeSqr = _config.AttackRange * _config.AttackRange;

        if (difference.sqrMagnitude <= attackRangeSqr)
        {
            _motor.FaceTarget(_enemy.Target.position);
            _stateMachine.ChangeState(_enemy.AttackState);
            return;
        }

        _motor.MoveTowards(_enemy.Target.position);
    }

    public void Exit()
    {
        
    }
}