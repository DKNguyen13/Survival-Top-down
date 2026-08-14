using UnityEngine;

[RequireComponent(typeof(PlayerInputReader), typeof(PlayerMotor), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerStatsConfig _config;

    private PlayerInputReader _inputReader;
    private PlayerMotor _motor;
    private PlayerStats _stats;
    private StateMachine _stateMachine;
    private PlayerIdleState _idleState;
    private PlayerMoveState _moveState;
    private PlayerDashState _dashState;
    private float _nextDashTime;

    public PlayerInputReader InputReader => _inputReader;
    public PlayerMotor Motor => _motor;
    public PlayerStatsConfig Config => _config;
    public PlayerIdleState IdleState => _idleState;
    public PlayerMoveState MoveState => _moveState;
    public PlayerDashState DashState => _dashState;
    public bool CanDash => Time.time >= _nextDashTime;

    private void Awake()
    {
        _inputReader = GetComponent<PlayerInputReader>();
        _motor = GetComponent<PlayerMotor>();
        _stats = GetComponent<PlayerStats>();

        if (_config == null)
        {
            Debug.LogError($"{nameof(PlayerController)} is missing {nameof(PlayerStatsConfig)}.", this);
            enabled = false;
            return;
        }

        _motor.Initialize(_config);
        _stats.Initialize(_config);

        _stateMachine = new StateMachine();
        _idleState = new PlayerIdleState(this, _stateMachine);
        _moveState = new PlayerMoveState(this, _stateMachine);
        _dashState = new PlayerDashState(this, _stateMachine);
    }

    private void OnEnable()
    {
        _inputReader.DashPressed += HandleDashPressed;
    }

    private void Start()
    {
        _stateMachine.Initialize(_idleState);
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    #region Handle event
    private void HandleDashPressed()
    {
        if (!CanDash) return;
        if (ReferenceEquals(_stateMachine.CurrentState, _dashState)) return;
        _stateMachine.ChangeState(_dashState);
    }
    #endregion

    #region Helper
    public void StartDashCooldown()
    {
        _nextDashTime = Time.time + _config.DashCooldown;
    }
    #endregion

    private void OnDisable()
    {
        _inputReader.DashPressed -= HandleDashPressed;
    }
}