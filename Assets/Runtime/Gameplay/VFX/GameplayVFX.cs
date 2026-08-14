using UnityEngine;

public class GameplayVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem[] _particles;
    [SerializeField] private TrailRenderer[] _trails;
    [SerializeField] private GameObject[] _visualObjects;

    [Header("Settings")]
    [SerializeField] private float _autoStopDuration;
    [SerializeField] private bool _disableVisualObjectsOnStop = true;

    private float _stopTime;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;

    private void Awake()
    {
        StopImmediate();
    }

    private void Update()
    {
        if (!_isPlaying) return;
        if (_autoStopDuration <= 0f) return;
        if (Time.time >= _stopTime)
        {
            Stop();
        }
    }

    public void Play()
    {
        _isPlaying = true;

        if (_autoStopDuration > 0f)
        {
            _stopTime = Time.time + _autoStopDuration;
        }

        SetVisualObjects(true);

        if (_trails != null)
        {
            for (int i = 0; i < _trails.Length; i++)
            {
                TrailRenderer trail = _trails[i];
                if (trail == null) continue;
                trail.Clear();
                trail.emitting = true;
            }
        }

        if (_particles != null)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                ParticleSystem particle = _particles[i];
                if (particle == null) continue;
                particle.Play(true);
            }
        }
    }

    public void Stop()
    {
        if (!_isPlaying) return;

        _isPlaying = false;

        if (_trails != null)
        {
            for (int i = 0; i < _trails.Length; i++)
            {
                if (_trails[i] != null)
                {
                    _trails[i].emitting = false;
                }
            }
        }

        if (_particles != null)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i] != null)
                {
                    _particles[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }

        if (_disableVisualObjectsOnStop)
        {
            SetVisualObjects(false);
        }
    }

    public void StopImmediate()
    {
        _isPlaying = false;

        if (_trails != null)
        {
            for (int i = 0; i < _trails.Length; i++)
            {
                TrailRenderer trail = _trails[i];
                if (trail == null) continue;
                trail.emitting = false;
                trail.Clear();
            }
        }

        if (_particles != null)
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                ParticleSystem particle = _particles[i];
                if (particle == null) continue;
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        if (_disableVisualObjectsOnStop)
        {
            SetVisualObjects(false);
        }
    }

    private void SetVisualObjects(bool active)
    {
        if (_visualObjects == null) return;

        for (int i = 0; i < _visualObjects.Length; i++)
        {
            if (_visualObjects[i] != null)
            {
                _visualObjects[i].SetActive(active);
            }
        }
    }

#if UNITY_EDITOR
    public void EditorSetup(ParticleSystem[] particles, TrailRenderer[] trails, GameObject[] visualObjects, float autoStopDuration)
    {
        _particles = particles;
        _trails = trails;
        _visualObjects = visualObjects;
        _autoStopDuration = autoStopDuration;
    }
#endif
}