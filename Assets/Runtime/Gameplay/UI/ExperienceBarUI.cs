using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBarUI : MonoBehaviour
{
    [SerializeField] private PlayerProgression _progression;
    [SerializeField] private Image _fill;
    [SerializeField] private TMP_Text _text;

    private void OnEnable()
    {
        if (_progression != null)
        {
            _progression.ExpChanged += HandleExperienceChanged;
        }
    }

    private void Start()
    {
        Refresh();
    }

    #region Handle event
    private void HandleExperienceChanged(int current, int required)
    {
        if (_fill != null)
        {
            _fill.fillAmount = required > 0 ? current / (float)required : 0f;
        }

        if (_text != null)
        {
            _text.text = $"{current}/{required}";
        }
    }
    #endregion

    private void Refresh()
    {
        if (_progression == null) return;
        HandleExperienceChanged(_progression.CurrentExp, _progression.ExpToLevelUp);
    }

    private void OnDisable()
    {
        if (_progression != null)
        {
            _progression.ExpChanged -= HandleExperienceChanged;
        }
    }
}