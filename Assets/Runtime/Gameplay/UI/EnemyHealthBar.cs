using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyHealth _health;
    [SerializeField] private HealthBarView _healthBar;

    private Transform _cameraTransform;

    private void OnEnable()
    {
        if (_health != null)
        {
            _health.HealthChanged += HandleHealthChanged;
        }
    }

    private void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            _cameraTransform = mainCamera.transform;
        }

        Refresh();
    }

    private void LateUpdate()
    {
        if (_cameraTransform == null) return;
        transform.rotation = _cameraTransform.rotation;
    }

    #region Handle event
    private void HandleHealthChanged(float current, float max)
    {
        _healthBar.SetValue(current, max);
    }
    #endregion

    private void Refresh()
    {
        if (_health == null) return;
        _healthBar.SetValue(_health.CurrentHealth, _health.MaxHealth, true);
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.HealthChanged -= HandleHealthChanged;
        }
    }
}