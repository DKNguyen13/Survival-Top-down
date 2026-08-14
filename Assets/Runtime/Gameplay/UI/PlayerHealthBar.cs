using TMPro;
using UnityEngine;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private PlayerProgression _progression;
    [SerializeField] private HealthBarView _healthBar;
    [SerializeField] private TMP_Text _levelText;

    private Transform _cameraTransform;

    private void OnEnable()
    {
        if (_stats != null)
        {
            _stats.HealthChanged += HandleHealthChanged;
        }

        if (_progression != null)
        {
            _progression.LevelChanged += HandleLevelChanged;
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

    private void HandleLevelChanged(int level)
    {
        if (_levelText != null)
        {
            _levelText.text = $"Lv.{level}";
        }
    }
    #endregion

    private void Refresh()
    {
        if (_stats != null)
        {
            _healthBar.SetValue(_stats.CurrentHealth, _stats.MaxHealth, true);
        }

        if (_progression != null)
        {
            HandleLevelChanged(_progression.Level);
        }
    }

    private void OnDisable()
    {
        if (_stats != null)
        {
            _stats.HealthChanged -= HandleHealthChanged;
        }

        if (_progression != null)
        {
            _progression.LevelChanged -= HandleLevelChanged;
        }
    }
}