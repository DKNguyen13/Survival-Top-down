using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    [SerializeField] private Image _fill;
    private EnemyHealth _health;
    private Transform _camera;

    private void Awake() => _health = GetComponentInParent<EnemyHealth>();

    private void OnEnable()
    {
        _camera = Camera.main != null ? Camera.main.transform : null;
        _health.HealthChanged += Refresh;
        Refresh(_health.CurrentHealth, _health.MaxHealth);
    }

    private void LateUpdate()
    {
        if (_camera != null) transform.rotation = _camera.rotation;
    }

    private void OnDisable()
    {
        if (_health != null) _health.HealthChanged -= Refresh;
    }

    private void Refresh(float current, float max) => _fill.fillAmount = max > 0f ? current / max : 0f;
}