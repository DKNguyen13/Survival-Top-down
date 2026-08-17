using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GameOverUI : MonoBehaviour
{
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private PlayerProgression _progression;
    [SerializeField] private WaveManager _waves;
    [SerializeField] private CanvasGroup _panel;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private Button _playAgainButton;

    private Coroutine _showRoutine;

    private void Awake()
    {
        SetVisible(false);
        _playAgainButton.onClick.AddListener(PlayAgain);
    }

    private void OnEnable()
    {
        _playerStats.Died += Show;
    }

    private void Show()
    {
        _resultText.text =
            $"LEVEL  {_progression.Level}\nWAVE  {_waves.Wave}";

        _panel.interactable = true;
        _panel.blocksRaycasts = true;

        if (_showRoutine != null) StopCoroutine(_showRoutine);
        _showRoutine = StartCoroutine(FadeIn());
        Time.timeScale = 0f;
    }

    private IEnumerator FadeIn()
    {
        const float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _panel.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        _panel.alpha = 1f;
        _showRoutine = null;
    }

    private void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetVisible(bool visible)
    {
        _panel.alpha = visible ? 1f : 0f;
        _panel.interactable = visible;
        _panel.blocksRaycasts = visible;
    }

    private void OnDisable()
    {
        if (_playerStats != null) _playerStats.Died -= Show;
    }

    private void OnDestroy()
    {
        if (_playAgainButton != null) _playAgainButton.onClick.RemoveListener(PlayAgain);
        Time.timeScale = 1f;
    }
}