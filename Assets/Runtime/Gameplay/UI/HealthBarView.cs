using DG.Tweening;
using UnityEngine;

public class HealthBarView : MonoBehaviour
{
    [SerializeField] private RectTransform _fill;
    [SerializeField] private float _duration = 0.2f;
    private Tween _tween;

    public void SetValue(float current, float max, bool instant = false)
    {
        if (_fill == null) return;
        float target = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        _tween?.Kill();

        if (instant)
        {
            SetNormalized(target);
            return;
        }

        float start = _fill.anchorMax.x;

        _tween = DOTween.To(() => start,
            value =>
            {
                start = value;
                SetNormalized(value);
            },
            target, _duration)
            .SetEase(Ease.OutQuad);
    }

    private void SetNormalized(float value)
    {
        Vector2 max = _fill.anchorMax;
        max.x = value;
        _fill.anchorMax = max;
    }

    private void OnDisable()
    {
        _tween?.Kill();
    }
}