using UnityEngine;

[RequireComponent(typeof(PlayerInputReader), typeof(PlayerMotor), typeof(PlayerStats))]
[RequireComponent(typeof(PlayerSkills), typeof(PlayerProgression))]
public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private PlayerStatsConfig _config;

    private PlayerInputReader _inputReader;
    private PlayerMotor _motor;
    private PlayerStats _stats;
    private PlayerSkills _skills;
    private PlayerProgression _progression;
    private StateMachine _stateMachine;
    private PlayerIdleState _idleState;
    private PlayerMoveState _moveState;
    private PlayerDashState _dashState;

    private void Awake()
    {
        _inputReader = GetComponent<PlayerInputReader>();
        _motor = GetComponent<PlayerMotor>();
        _stats = GetComponent<PlayerStats>();
        _skills = GetComponent<PlayerSkills>();
        _progression = GetComponent<PlayerProgression>();

        if (_config == null)
        {
            Debug.LogError($"{nameof(PlayerController)} is missing {nameof(PlayerStatsConfig)}.", this);
            enabled = false;
            return;
        }

        _motor.Initialize(_config);
        _stats.Initialize(_config);
        _skills.Initialize(_config, _stats, _inputReader);
        _progression.Initialize(_config, _stats);

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
        if (ReferenceEquals(_stateMachine.CurrentState, _dashState)) return;
        if (!_skills.TryStartDash()) return;
        _stateMachine.ChangeState(_dashState);
    }

    public void PressShootUI()
    {
        InputReader.PressShoot();
    }

    public void PressBombUI()
    {
        InputReader.PressBomb();
    }

    public void PressDashUI()
    {
        InputReader.PressDash();
    }
    #endregion

    private void OnDisable()
    {
        _inputReader.DashPressed -= HandleDashPressed;
    }

    // Getter, Setter
    public PlayerInputReader InputReader => _inputReader;
    public PlayerMotor Motor => _motor;
    public PlayerStatsConfig Config => _config;
    public PlayerStats Stats => _stats;
    public PlayerSkills Skills => _skills;
    public PlayerIdleState IdleState => _idleState;
    public PlayerMoveState MoveState => _moveState;
    public PlayerDashState DashState => _dashState;
}
