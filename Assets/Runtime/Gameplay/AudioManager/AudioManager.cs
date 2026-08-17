using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioLibrary _library;
    [SerializeField, Min(4)] private int _sfxPoolSize = 12;
    [SerializeField, Range(0f, 1f)] private float _sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _bgmVolume = 1f;

    private static AudioManager _instance;
    private readonly List<AudioSource> _sfxSources = new();
    private readonly Dictionary<SfxId, float> _lastPlayedAt = new();
    private AudioSource _bgmSource;
    private Coroutine _bgmRoutine;
    private int _nextSource;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        CreateSources();
    }

    private void Start()
    {
        PlayBgm(BgmId.Gameplay, 0.5f);
    }

    public static void PlaySfx(SfxId id, Vector3 position)
    {
        _instance?.PlaySfxInternal(id, position, false);
    }

    public static void PlayUI(SfxId id)
    {
        _instance?.PlaySfxInternal(id, Vector3.zero, true);
    }

    public static void PlayBgm(BgmId id, float fadeDuration = 0.5f)
    {
        if (_instance == null || !_instance._library.TryGetBgm(id, out AudioLibrary.BgmEntry entry)) return;
        if (_instance._bgmSource.clip == entry.clip && _instance._bgmSource.isPlaying) return;

        if (_instance._bgmRoutine != null)
        {
            _instance.StopCoroutine(_instance._bgmRoutine);
        }
        _instance._bgmRoutine = _instance.StartCoroutine(_instance.ChangeBgm(entry, fadeDuration));
    }

    private void PlaySfxInternal(SfxId id, Vector3 position, bool force2D)
    {
        if (_library == null || !_library.TryGetSfx(id, out AudioLibrary.SfxEntry entry)) return;

        float now = Time.unscaledTime;
        if (_lastPlayedAt.TryGetValue(id, out float previous) && now - previous < entry.minInterval) return;
        _lastPlayedAt[id] = now;

        AudioSource source = GetAvailableSource();
        source.transform.position = position;
        source.spatialBlend = force2D ? 0f : entry.spatialBlend;
        source.pitch = Random.Range(entry.minPitch, entry.maxPitch);
        source.volume = entry.volume * _sfxVolume;
        source.clip = entry.clip;
        source.Play();
    }

    private AudioSource GetAvailableSource()
    {
        for (int i = 0; i < _sfxSources.Count; i++)
        {
            if (!_sfxSources[i].isPlaying)
            {
                return _sfxSources[i]; 
            }
        }

        AudioSource source = _sfxSources[_nextSource];
        _nextSource = (_nextSource + 1) % _sfxSources.Count;
        source.Stop();
        return source;
    }

    private void CreateSources()
    {
        for (int i = 0; i < _sfxPoolSize; i++)
        {
            GameObject child = new($"SFX {i + 1}");
            child.transform.SetParent(transform, false);
            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 2f;
            source.maxDistance = 18f;
            _sfxSources.Add(source);
        }

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;
        _bgmSource.spatialBlend = 0f;
    }

    private IEnumerator ChangeBgm(AudioLibrary.BgmEntry entry, float duration)
    {
        yield return FadeBgm(0f, duration * 0.5f);

        _bgmSource.clip = entry.clip;
        _bgmSource.volume = 0f;
        _bgmSource.Play();

        yield return FadeBgm(entry.volume * _bgmVolume, duration * 0.5f);
        _bgmRoutine = null;
    }

    private IEnumerator FadeBgm(float target, float duration)
    {
        float start = _bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _bgmSource.volume = Mathf.Lerp(start, target, duration <= 0f ? 1f : elapsed / duration);
            yield return null;
        }

        _bgmSource.volume = target;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }
}