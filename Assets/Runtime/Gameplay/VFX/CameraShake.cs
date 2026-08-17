using Cinemachine;
using UnityEngine;

public sealed class CameraShake : MonoBehaviour
{
    private static CameraShake _instance;
    private CinemachineImpulseSource _source;
    private CinemachineImpulseListener _listener;

    public static void Play(Vector3 position, float strength)
    {
        if (strength <= 0f) return;

        EnsureInstance();
        _instance.EnsureListener();

        _instance.transform.position = position;
        Vector2 direction = Random.insideUnitCircle.normalized;
        _instance._source.GenerateImpulseWithVelocity(new Vector3(direction.x, direction.y, 0f) * strength);
    }

    private static void EnsureInstance()
    {
        if (_instance != null) return;

        GameObject root = new("[CameraShake]");
        DontDestroyOnLoad(root);
        _instance = root.AddComponent<CameraShake>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        _source = gameObject.AddComponent<CinemachineImpulseSource>();
        _source.m_DefaultVelocity = Vector3.down;

        CinemachineImpulseDefinition impulse = _source.m_ImpulseDefinition;
        impulse.m_ImpulseChannel = 1;
        impulse.m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
        impulse.m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
        impulse.m_ImpulseDuration = 0.22f;
        impulse.m_AmplitudeGain = 1f;
        impulse.m_FrequencyGain = 1.2f;
    }

    private void EnsureListener()
    {
        if (_listener != null) return;

        CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        if (virtualCamera == null) return;

        _listener = virtualCamera.GetComponent<CinemachineImpulseListener>();
        if (_listener == null)
        {
            _listener = virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
        }

        _listener.m_ChannelMask = 1;
        _listener.m_Gain = 1f;
        _listener.m_UseCameraSpace = true;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}