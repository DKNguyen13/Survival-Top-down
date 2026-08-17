using UnityEngine;

[RequireComponent(typeof(EnemyMotor), typeof(EnemyHealth))]
public class EnemyController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private EnemyStatsConfig _config;

    private Transform _target;
    private EnemyMotor _motor;
    private EnemyAttack _attack;
    private EnemyHealth _health;
    private StateMachine _stateMachine;
    private EnemyChaseState _chaseState;
    private EnemyAttackState _attackState;
    private EnemyRecoveryState _recoveryState;
    private IDamageable _targetDamageable;

    private void Awake()
    {
        _motor = GetComponent<EnemyMotor>();
        _attack = GetComponent<EnemyAttack>();
        _health = GetComponent<EnemyHealth>();

        if (_config == null || _attack == null)
        {
            Debug.LogError($"{nameof(EnemyController)} requires a config and an {nameof(EnemyAttack)}.", this);
            enabled = false;
            return;
        }

        _motor.Initialize(_config);
        _attack.Initialize(_config);
        _health.Initialize(_config);

        _stateMachine = new StateMachine();
        _chaseState = new EnemyChaseState(this, _stateMachine);
        _attackState = new EnemyAttackState(this, _stateMachine);
        _recoveryState = new EnemyRecoveryState(this, _stateMachine);
    }

    private void Start()
    {
        CacheTargetDamageable();
        _stateMachine.Initialize(_chaseState);
    }

    private void Update()
    {
        if (_health.IsAlive) _stateMachine.Update();
    }

    public void Initialize(Transform target)
    {
        _target = target;
        _targetDamageable = target != null ? target.GetComponent<IDamageable>() : null;
        _health.ResetHealth();
        _stateMachine.Initialize(_chaseState);
    }

    private void CacheTargetDamageable()
    {
        if (_target == null)
        {
            _targetDamageable = null;
            return;
        }

        _targetDamageable = _target.GetComponent<IDamageable>();
    }

    // Getter, Setter
    public EnemyMotor Motor => _motor;
    public EnemyAttack Attack => _attack;
    public EnemyHealth Health => _health;
    public EnemyStatsConfig Config => _config;
    public Transform Target => _target;
    public IDamageable TargetDamageable => _targetDamageable;
    public EnemyChaseState ChaseState => _chaseState;
    public EnemyAttackState AttackState => _attackState;
    public EnemyRecoveryState RecoveryState => _recoveryState;
}
