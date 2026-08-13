using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader _inputReader;
    [SerializeField] private PlayerMotor _motor;

    private StateMachine stateMachine;
    private PlayerIdleState idleState;
    private PlayerMoveState moveState;
    private PlayerDashState dashState;
    private float _nextDashTime;

    public PlayerInputReader InputReader => _inputReader;
    public PlayerMotor Motor => _motor;
    public PlayerIdleState IdleState => idleState;
    public PlayerMoveState MoveState => moveState;
    public PlayerDashState DashState => dashState;
    public bool CanDash => Time.time >= _nextDashTime;

    private void Awake()
    {
        stateMachine = new StateMachine();
        idleState = new PlayerIdleState(this, stateMachine);
        moveState = new PlayerMoveState(this, stateMachine);
        dashState = new PlayerDashState(this, stateMachine);
    }

    private void OnEnable()
    {
        _inputReader.DashPressed += HandleDashPressed;
    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        stateMachine.Update();
    }

    #region Handle event
    private void HandleDashPressed()
    {
        if (!CanDash) return;
        if (ReferenceEquals(stateMachine.CurrentState, dashState)) return;
        stateMachine.ChangeState(dashState);
    }
    #endregion

    #region Helper
    public void StartDashCooldown()
    {
        _nextDashTime = Time.time + _motor.Config.DashCooldown;
    }
    #endregion

    private void OnDisable()
    {
        _inputReader.DashPressed -= HandleDashPressed;
    }
}