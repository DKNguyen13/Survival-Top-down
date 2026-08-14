using UnityEngine;

public class EnemyRecoveryState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private readonly EnemyStatsConfig _config;
    private float _elapsedTime;

    public EnemyRecoveryState(
        EnemyController enemy,
        StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
        _config = enemy.Config;
    }

    public void Enter()
    {
        _elapsedTime = 0f;
    }

    public void Update()
    {
        if (_enemy.Target == null) return;
        _enemy.Motor.FaceTarget(_enemy.Target.position);
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= _config.RecoveryDuration)
        {
            _stateMachine.ChangeState(_enemy.ChaseState);
        }
    }

    public void Exit()
    {
        
    }
}