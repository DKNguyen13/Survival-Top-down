using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyMotor _motor;
    [SerializeField] private Transform _target;

    private StateMachine _stateMachine;
    private EnemyChaseState _chaseState;
    public EnemyMotor Motor => _motor;
    public Transform Target => _target;

    private void Awake()
    {
        _stateMachine = new StateMachine();
        _chaseState = new EnemyChaseState(this, _stateMachine);
    }

    private void Start()
    {
        _stateMachine.Initialize(_chaseState);
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    public void Initialize(Transform target)
    {
        _target = target;
    }
}