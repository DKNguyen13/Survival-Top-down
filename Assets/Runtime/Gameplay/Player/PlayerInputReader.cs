using System;
using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public event Action ShootPressed;
    public event Action BombPressed;
    public event Action DashPressed;
    private Vector2 _joystickInput;
    private bool _joystickActive;

    private void Update()
    {
        ReadMovementInput();

#if UNITY_EDITOR
        ReadKeyboardActions();
#endif
    }

    #region Movement Input
    private void ReadMovementInput()
    {
#if UNITY_EDITOR
        if (!_joystickActive)
        {
            Vector2 keyboardInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            MoveInput = Vector2.ClampMagnitude(keyboardInput, 1f);
            return;
        }
#endif

        MoveInput = _joystickActive ? _joystickInput : Vector2.zero;
    }

    public void SetJoystickInput(Vector2 input)
    {
        _joystickActive = true;
        _joystickInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void ReleaseJoystick()
    {
        _joystickActive = false;
        _joystickInput = Vector2.zero;
        MoveInput = Vector2.zero;
    }
    #endregion

    #region Skill Input
    public void PressShoot() => ShootPressed?.Invoke();
    public void PressBomb() => BombPressed?.Invoke();
    public void PressDash() => DashPressed?.Invoke();

#if UNITY_EDITOR
    private void ReadKeyboardActions()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            BombPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            DashPressed?.Invoke();
        }
    }
#endif
    #endregion
}