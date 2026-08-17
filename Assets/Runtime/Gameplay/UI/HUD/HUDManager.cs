using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerController _player;
    [SerializeField] private PlayerProgression _progression;
    [SerializeField] private WaveManager _waves;

    [Header("Bars")]
    [SerializeField] private Image _healthFill;
    [SerializeField] private Image _expFill;
    [SerializeField] private Image _bombCooldown;
    [SerializeField] private Image _dashCooldown;
    [SerializeField] private Image _shootCooldown;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _shootText;
    [SerializeField] private TextMeshProUGUI _bombText;
    [SerializeField] private TextMeshProUGUI _dashText;

    [Header("Animation")]
    [SerializeField] private float _barDuration = 0.2f;
    [SerializeField] private float _popScale = 1.15f;
    [SerializeField] private float _popDuration = 0.12f;

    private Tween _healthTween;
    private Tween _expTween;
    private Tween _levelTween;
    private Tween _chargeTween;
    private Tween _waveTween;

    private void Start()
    {
        SubscribeEvents();
        RefreshInitialUI();
    }

    private void Update()
    {
        RefreshCooldowns();
    }

    private void RefreshInitialUI()
    {
        _healthFill.fillAmount = _player.Stats.MaxHealth > 0f ? _player.Stats.CurrentHealth / _player.Stats.MaxHealth : 0f;
        _expFill.fillAmount = _progression.ExpToLevelUp > 0 ? _progression.CurrentExp / (float)_progression.ExpToLevelUp : 0f;
        _levelText.text = $"LEVEL {_progression.Level}";
        _waveText.text = $"WAVE {_waves.Wave}   ENEMIES {_waves.AliveEnemies}";
        RefreshCooldowns();
    }

    private void RefreshCooldowns()
    {
        RefreshCooldown(_bombCooldown, _bombText, _player.Skills.BombCooldown01, _player.Skills.BombCooldownRemaining);
        RefreshCooldown(_dashCooldown, _dashText, _player.Skills.DashCooldown01, _player.Skills.DashCooldownRemaining);
        RefreshShootCooldown();
    }

    private void RefreshCooldown(Image image, TextMeshProUGUI text, float fill, float remaining)
    {
        bool active = remaining > 0f;
        image.gameObject.SetActive(active);
        text.gameObject.SetActive(active);

        if (!active) return;
        image.fillAmount = fill;
        text.text = remaining.ToString("0.0");
    }

    private void RefreshShootCooldown()
    {
        int current = _player.Skills.ShootCharges;
        int max = _player.Skills.MaxShootCharges;
        bool recharging = current < max;

        _shootCooldown.gameObject.SetActive(recharging);
        _shootText.text = $"{current}/{max}";

        if (!recharging)return;

        _shootCooldown.fillAmount = _player.Skills.ShootChargeCooldown01;

        Color color = _shootCooldown.color;
        color.a = current == 0 ? 200f / 255f : 100f / 255f;
        _shootCooldown.color = color;
    }

    private void SubscribeEvents()
    {
        _player.Stats.HealthChanged += OnRefreshHealth;
        _progression.LevelChanged += OnRefreshLevel;
        _progression.ExpChanged += OnRefreshExperience;
        _player.Skills.ShootChargesChanged += OnRefreshCharges;
        _waves.WaveChanged += OnRefreshWave;
    }

    #region Handle Event
    private void OnRefreshHealth(float current, float max)
    {
        float target = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        _healthTween?.Kill();
        _healthTween = _healthFill.DOFillAmount(target, _barDuration).SetEase(Ease.OutQuad);
    }

    private void OnRefreshExperience(int current, int required)
    {
        float target = required > 0 ? Mathf.Clamp01(current / (float)required) : 0f;
        _expTween?.Kill();
        _expTween = _expFill
            .DOFillAmount(target, _barDuration)
            .SetEase(Ease.OutQuad);
    }

    private void OnRefreshLevel(int level)
    {
        _levelText.text = $"LEVEL {level}";
        PlayPop(_levelText.rectTransform, ref _levelTween, 1.25f);
    }

    private void OnRefreshCharges(int current, int max)
    {
        RefreshShootCooldown();
        PlayPop(_shootText.rectTransform, ref _chargeTween, _popScale);
    }

    private void OnRefreshWave(int wave, int alive)
    {
        _waveText.text = $"WAVE {wave}   ENEMIES {alive}";
        PlayPop(_waveText.rectTransform, ref _waveTween, 1.08f);
    }
    #endregion

    private void PlayPop(RectTransform target, ref Tween tween, float scale)
    {
        if (target == null) return;
        tween?.Kill();
        target.localScale = Vector3.one;
        tween = target
            .DOScale(scale, _popDuration)
            .SetEase(Ease.OutQuad)
            .SetLoops(2, LoopType.Yoyo);
    }

    private void KillTweens()
    {
        _healthTween?.Kill();
        _expTween?.Kill();
        _levelTween?.Kill();
        _chargeTween?.Kill();
        _waveTween?.Kill();
    }

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.Stats.HealthChanged -= OnRefreshHealth;
            _player.Skills.ShootChargesChanged -= OnRefreshCharges;
        }

        if (_progression != null)
        {
            _progression.LevelChanged -= OnRefreshLevel;
            _progression.ExpChanged -= OnRefreshExperience;
        }

        if (_waves != null)
        {
            _waves.WaveChanged -= OnRefreshWave;
        }

        KillTweens();
    }
}