using System.Collections.Generic;
using UnityEngine;

public sealed class PrototypeEffects : MonoBehaviour
{
    private enum FxType
    {
        Hit,
        Bomb,
        Dash,
        MeleeAttack,
        LevelUp,
        PoisonHit,
    }

    private sealed class RingFx
    {
        public Transform Transform;
        public LineRenderer Line;

        public float Delay;
        public float Duration;
        public float StartScale;
        public float EndScale;
        public float StartWidth;
        public Color Color;
    }

    private sealed class FxInstance
    {
        public FxType Type;
        public GameObject Root;

        public ParticleSystem Core;
        public ParticleSystem Sparks;
        public ParticleSystem Glints;
        public ParticleSystem Smoke;
        public ParticleSystem Rise;

        public RingFx[] Rings;

        public float StartTime;
        public float Lifetime;
    }

    private static PrototypeEffects _instance;

    private readonly Dictionary<FxType, Queue<FxInstance>> _pools = new();
    private readonly List<FxInstance> _active = new();

    private Material _glowMaterial;
    private Material _sparkMaterial;
    private Material _smokeMaterial;
    private Material _lineMaterial;

    private Texture2D _glowTexture;
    private Texture2D _sparkTexture;

    // =========================================================
    // AUTO INSTALL
    // =========================================================

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInstall()
    {
        if (_instance != null) return;

        GameObject root = new("[PrototypeEffects]");
        root.AddComponent<PrototypeEffects>();
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        CreateRuntimeResources();
        CreatePools();
        Prewarm();
    }

    private void Update()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            FxInstance fx = _active[i];
            float elapsed = Time.time - fx.StartTime;

            UpdateRings(fx, elapsed);

            if (elapsed < fx.Lifetime) continue;

            ReturnToPool(fx);
            _active.RemoveAt(i);
        }
    }

    private void OnDestroy()
    {
        if (_instance != this) return;

        _instance = null;

        DestroyRuntimeObject(_glowMaterial);
        DestroyRuntimeObject(_sparkMaterial);
        DestroyRuntimeObject(_smokeMaterial);
        DestroyRuntimeObject(_lineMaterial);
        DestroyRuntimeObject(_glowTexture);
        DestroyRuntimeObject(_sparkTexture);
    }

    // =========================================================
    // PUBLIC API
    // =========================================================

    public static void PlayHit(Vector3 position, Color color)
    {
        if (_instance == null) return;
        _instance.SpawnHit(position, color);
    }

    public static void PlayPoisonHit(Vector3 position, Color color)
    {
        if (_instance == null) return;
        _instance.SpawnPoisonHit(position, color);
    }

    public static void PlayBomb(Vector3 position, float radius, Color color)
    {
        if (_instance == null) return;
        _instance.SpawnBomb(position, radius, color);
    }

    public static void PlayDash(Vector3 position, Vector3 direction, Color color)
    {
        if (_instance == null) return;
        _instance.SpawnDash(position, direction, color);
    }

    public static void PlayMeleeAttack(Vector3 position, Vector3 direction, Color color)
    {
        if (_instance == null) return;
        _instance.SpawnMeleeAttack(position, direction, color);
    }

    public static void PlayLevelUp(Vector3 position, Color color)
    {
        if (_instance == null) return;
        _instance.SpawnLevelUp(position, color);
    }

    // =========================================================
    // SPAWN HIT
    // =========================================================

    private void SpawnHit(Vector3 position, Color color)
    {
        FxInstance fx = GetEffect(FxType.Hit);

        fx.Root.transform.SetPositionAndRotation(position + Vector3.up * 0.15f, Quaternion.identity);

        SetParticleColor(fx.Core, Color.Lerp(Color.white, color, 0.25f));
        SetParticleColor(fx.Sparks, Color.Lerp(Color.white, color, 0.35f));
        SetParticleColor(fx.Glints, color);

        SetupRing(fx.Rings[0], 0.05f, 0.65f, 0.055f, color, 0f, 0.22f);

        PlayEffect(fx, 0.45f);
    }

    // =========================================================
    // SPAWN BOMB
    // =========================================================

    private void SpawnBomb(Vector3 position, float radius, Color color)
    {
        FxInstance fx = GetEffect(FxType.Bomb);

        fx.Root.transform.SetPositionAndRotation(position + Vector3.up * 0.08f, Quaternion.identity);

        Color hotColor = Color.Lerp(Color.white, color, 0.3f);

        SetParticleColor(fx.Core, hotColor);
        SetParticleColor(fx.Sparks, color);
        SetParticleColor(fx.Glints, Color.Lerp(Color.white, color, 0.5f));

        ParticleSystem.MainModule sparkMain = fx.Sparks.main;
        sparkMain.startSpeed = new ParticleSystem.MinMaxCurve(
            Mathf.Max(2.5f, radius * 0.65f),
            Mathf.Max(4.5f, radius * 1.1f));

        ParticleSystem.ShapeModule smokeShape = fx.Smoke.shape;
        smokeShape.radius = Mathf.Clamp(radius * 0.12f, 0.15f, 0.75f);

        SetupRing(fx.Rings[0], 0.08f, radius * 0.82f,Mathf.Clamp(radius * 0.025f, 0.055f, 0.14f), Color.Lerp(Color.white, color, 0.35f), 0f, 0.28f);
        SetupRing(fx.Rings[1],0.05f, radius, Mathf.Clamp(radius * 0.035f, 0.07f, 0.18f), color, 0.035f, 0.48f);
        PlayEffect(fx, 1.15f);
    }

    // =========================================================
    // SPAWN DASH
    // =========================================================

    private void SpawnDash(Vector3 position, Vector3 direction, Color color)
    {
        FxInstance fx = GetEffect(FxType.Dash);

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.forward;

        direction.y = 0f;
        direction.Normalize();

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

        fx.Root.transform.SetPositionAndRotation(position + Vector3.up * 0.1f, rotation);
        fx.Root.transform.localScale = Vector3.one;

        SetParticleColor(fx.Sparks, Color.Lerp(Color.white, color, 0.2f));
        SetParticleColor(fx.Glints, color);

        SetupRing(fx.Rings[0], 0.06f, 0.75f, 0.045f, color, 0f, 0.25f);

        PlayEffect(fx, 0.65f);
    }
    private void SpawnMeleeAttack(Vector3 position, Vector3 direction, Color color)
    {
        FxInstance fx = GetEffect(FxType.MeleeAttack);

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) direction = Vector3.forward;
        direction.Normalize();

        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

        fx.Root.transform.SetPositionAndRotation(position + direction * 0.65f + Vector3.up * 0.5f, rotation);
        fx.Root.transform.localScale = new Vector3(1.55f, 1.05f, 1.15f);

        SetParticleColor(fx.Sparks, Color.Lerp(Color.white, color, 0.15f));
        SetParticleColor(fx.Glints, Color.Lerp(Color.white, color, 0.25f));
        SetParticleColor(fx.Smoke, Color.Lerp(color, Color.black, 0.2f), 0.45f);

        SetupRing(fx.Rings[0], 0.1f, 1.15f, 0.11f, color, 0f, 0.28f);

        PlayEffect(fx, 0.65f);
    }
    // =========================================================
    // SPAWN LEVEL UP
    // =========================================================

    private void SpawnLevelUp(Vector3 position, Color color)
    {
        FxInstance fx = GetEffect(FxType.LevelUp);

        fx.Root.transform.SetPositionAndRotation(position + Vector3.up * 0.08f, Quaternion.identity);

        SetParticleColor(fx.Core, Color.Lerp(Color.white, color, 0.25f));
        SetParticleColor(fx.Glints, color);
        SetParticleColor(fx.Rise, Color.Lerp(Color.white, color, 0.35f));

        SetupRing(fx.Rings[0], 0.1f, 1.4f, 0.065f, color, 0f, 0.55f);
        SetupRing(fx.Rings[1], 0.05f, 0.95f, 0.045f, Color.white, 0.12f, 0.45f);

        PlayEffect(fx, 1.4f);
    }

    private void SpawnPoisonHit(Vector3 position, Color color)
    {
        FxInstance fx = GetEffect(FxType.PoisonHit);

        fx.Root.transform.SetPositionAndRotation(
            position + Vector3.up * 0.45f,
            Quaternion.identity);

        Color toxic = Color.Lerp(
            color,
            new Color(0.65f, 1f, 0.05f),
            0.35f);

        Color darkPoison = Color.Lerp(
            color,
            new Color(0.12f, 0.02f, 0.18f),
            0.25f);

        SetParticleColor(fx.Core, toxic, 1f);
        SetParticleColor(fx.Sparks, toxic, 1f);
        SetParticleColor(fx.Glints, toxic, 1f);
        SetParticleColor(fx.Smoke, darkPoison, 0.8f);
        SetParticleColor(fx.Rise, toxic, 1f);
        PlayEffect(fx, 1.15f);
    }

    // =========================================================
    // PLAY
    // =========================================================

    private void PlayEffect(FxInstance fx, float lifetime)
    {
        fx.StartTime = Time.time;
        fx.Lifetime = lifetime;
        fx.Root.SetActive(true);

        PlayParticle(fx.Core);
        PlayParticle(fx.Sparks);
        PlayParticle(fx.Glints);
        PlayParticle(fx.Smoke);
        PlayParticle(fx.Rise);

        _active.Add(fx);
    }

    private static void PlayParticle(ParticleSystem particle)
    {
        if (particle == null) return;

        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particle.Play(true);
    }

    // =========================================================
    // RING
    // =========================================================

    private static void SetupRing(
        RingFx ring,
        float startScale,
        float endScale,
        float width,
        Color color,
        float delay,
        float duration)
    {
        if (ring == null) return;

        ring.StartScale = startScale;
        ring.EndScale = endScale;
        ring.StartWidth = width;
        ring.Color = color;
        ring.Delay = delay;
        ring.Duration = duration;

        ring.Transform.localScale = Vector3.one * startScale;

        ring.Line.startWidth = width;
        ring.Line.endWidth = width;

        Color hidden = color;
        hidden.a = 0f;

        ring.Line.startColor = hidden;
        ring.Line.endColor = hidden;
        ring.Line.enabled = false;
    }

    private static void UpdateRings(FxInstance fx, float elapsed)
    {
        if (fx.Rings == null) return;

        for (int i = 0; i < fx.Rings.Length; i++)
        {
            RingFx ring = fx.Rings[i];
            if (ring == null) continue;

            float ringElapsed = elapsed - ring.Delay;

            if (ringElapsed < 0f)
            {
                ring.Line.enabled = false;
                continue;
            }

            float progress = ringElapsed / ring.Duration;

            if (progress >= 1f)
            {
                ring.Line.enabled = false;
                continue;
            }

            ring.Line.enabled = true;
            progress = Mathf.Clamp01(progress);

            // Ease Out Cubic: bung nhanh đầu, chậm dần cuối.
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            float scale = Mathf.Lerp(ring.StartScale, ring.EndScale, eased);

            ring.Transform.localScale = Vector3.one * scale;

            float widthMultiplier = Mathf.Lerp(1.35f, 0.25f, progress);
            float width = ring.StartWidth * widthMultiplier;

            ring.Line.startWidth = width;
            ring.Line.endWidth = width;

            Color color = ring.Color;
            color.a = Mathf.Pow(1f - progress, 1.6f);

            ring.Line.startColor = color;
            ring.Line.endColor = color;
        }
    }

    // =========================================================
    // POOL
    // =========================================================

    private void CreatePools()
    {
        foreach (FxType type in System.Enum.GetValues(typeof(FxType)))
            _pools[type] = new Queue<FxInstance>();
    }

    private void Prewarm()
    {
        Prewarm(FxType.Hit, 12);
        Prewarm(FxType.Bomb, 4);
        Prewarm(FxType.Dash, 6);
        Prewarm(FxType.MeleeAttack, 6);
        Prewarm(FxType.LevelUp, 2);
        Prewarm(FxType.PoisonHit, 8);
    }

    private void Prewarm(FxType type, int count)
    {
        Queue<FxInstance> pool = _pools[type];

        for (int i = 0; i < count; i++)
        {
            FxInstance fx = CreateEffect(type);
            fx.Root.SetActive(false);
            pool.Enqueue(fx);
        }
    }

    private FxInstance GetEffect(FxType type)
    {
        Queue<FxInstance> pool = _pools[type];

        if (pool.Count > 0)
            return pool.Dequeue();

        return CreateEffect(type);
    }

    private void ReturnToPool(FxInstance fx)
    {
        StopParticle(fx.Core);
        StopParticle(fx.Sparks);
        StopParticle(fx.Glints);
        StopParticle(fx.Smoke);
        StopParticle(fx.Rise);

        if (fx.Rings != null)
        {
            foreach (RingFx ring in fx.Rings)
            {
                if (ring != null)
                    ring.Line.enabled = false;
            }
        }

        fx.Root.SetActive(false);
        _pools[fx.Type].Enqueue(fx);
    }

    private static void StopParticle(ParticleSystem particle)
    {
        if (particle == null) return;

        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    // =========================================================
    // CREATE EFFECT
    // =========================================================

    private FxInstance CreateEffect(FxType type)
    {
        GameObject root = new(type + "_VFX");
        root.transform.SetParent(transform);

        FxInstance fx = new()
        {
            Type = type,
            Root = root
        };

        switch (type)
        {
            case FxType.Hit:
                BuildHit(fx);
                break;

            case FxType.Bomb:
                BuildBomb(fx);
                break;

            case FxType.Dash:
                BuildDash(fx);
                break;

            case FxType.MeleeAttack:
                BuildDash(fx);
                break;

            case FxType.LevelUp:
                BuildLevelUp(fx);
                break;
            case FxType.PoisonHit:
                BuildPoisonHit(fx);
                break;
        }

        return fx;
    }

    // =========================================================
    // BUILD
    // =========================================================

    private void BuildHit(FxInstance fx)
    {
        fx.Core = CreateCoreFlash(fx.Root.transform, 0.28f, 0.11f);
        fx.Sparks = CreateSparks(fx.Root.transform, 9, 2.2f, 4.5f, 0.025f, 0.065f);
        fx.Glints = CreateGlints(fx.Root.transform, 4, 0.6f, 1.5f);

        fx.Rings = new[]
        {
            CreateRing(fx.Root.transform, "HitRing")
        };
    }

    private void BuildBomb(FxInstance fx)
    {
        fx.Core = CreateCoreFlash(fx.Root.transform, 1.35f, 0.16f);
        fx.Sparks = CreateSparks(fx.Root.transform, 22, 3f, 6f, 0.035f, 0.11f);
        fx.Glints = CreateGlints(fx.Root.transform, 10, 1f, 3f);
        fx.Smoke = CreateSmoke(fx.Root.transform, 7);

        fx.Rings = new[]
        {
            CreateRing(fx.Root.transform, "BombRingInner"),
            CreateRing(fx.Root.transform, "BombRingOuter")
        };
    }

    private void BuildDash(FxInstance fx)
    {
        fx.Sparks = CreateDashStreaks(fx.Root.transform);
        fx.Glints = CreateGlints(fx.Root.transform, 7, 0.5f, 1.8f);
        fx.Smoke = CreateDashDust(fx.Root.transform);

        fx.Rings = new[]
        {
            CreateRing(fx.Root.transform, "DashRing")
        };
    }

    private void BuildLevelUp(FxInstance fx)
    {
        fx.Core = CreateCoreFlash(fx.Root.transform, 1.25f, 0.24f);
        fx.Glints = CreateGlints(fx.Root.transform, 14, 0.7f, 2.4f);
        fx.Rise = CreateRisingParticles(fx.Root.transform);

        fx.Rings = new[]
        {
            CreateRing(fx.Root.transform, "LevelRingOuter"),
            CreateRing(fx.Root.transform, "LevelRingInner")
        };
    }

    private void BuildPoisonHit(FxInstance fx)
    {
        fx.Sparks = CreatePoisonSplash(fx.Root.transform);
        fx.Glints = CreatePoisonDroplets(fx.Root.transform);
        fx.Smoke = CreatePoisonCloud(fx.Root.transform);
        fx.Rise = CreatePoisonBubbles(fx.Root.transform);
        fx.Rings = null;
    }

    private ParticleSystem CreatePoisonSplash(Transform parent)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "PoisonSplash", _sparkMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 0.25f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.42f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
        main.maxParticles = 16;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 65f;
        shape.radius = 0.08f;
        shape.rotation = new Vector3(-90f, 0f, 0f);

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);

        SetBurst(ps, 12);
        SetFadeCurve(ps, 1f, 0f);

        SetSizeCurve(ps, new AnimationCurve(
            new Keyframe(0f, 0.4f),
            new Keyframe(0.1f, 1f),
            new Keyframe(0.7f, 0.65f),
            new Keyframe(1f, 0f)));

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 1.8f;
        renderer.velocityScale = 0.12f;

        return ps;
    }

    private ParticleSystem CreatePoisonCloud(Transform parent)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "PoisonCloud", _smokeMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 0.65f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.65f, 1.1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.65f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.28f, 0.65f);
        main.maxParticles = 12;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.18f;

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;

        velocity.x = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.35f, 1.1f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.35f;
        noise.frequency = 0.8f;
        noise.scrollSpeed = 0.3f;
        noise.quality = ParticleSystemNoiseQuality.Low;

        SetBurst(ps, 8);
        SetFadeCurve(ps, 0.9f, 0f);

        SetSizeCurve(ps, new AnimationCurve(
            new Keyframe(0f, 0.2f),
            new Keyframe(0.25f, 0.75f),
            new Keyframe(0.65f, 1.1f),
            new Keyframe(1f, 1.3f)));

        return ps;
    }

    private ParticleSystem CreatePoisonDroplets(Transform parent)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "PoisonDroplets", _glowMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 0.35f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.55f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.13f);
        main.maxParticles = 14;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.13f;

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);

        SetBurst(ps, 10);
        SetFadeCurve(ps, 0.95f, 0f);

        SetSizeCurve(ps, new AnimationCurve(
            new Keyframe(0f, 0.35f),
            new Keyframe(0.15f, 1f),
            new Keyframe(0.7f, 0.75f),
            new Keyframe(1f, 0f)));

        return ps;
    }

    private ParticleSystem CreatePoisonMist(Transform parent)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "PoisonMist", _smokeMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 0.55f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.55f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.65f);
        main.maxParticles = 10;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.18f;

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;

        velocity.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.35f, 0.9f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);

        SetBurst(ps, 6);
        SetFadeCurve(ps, 0.38f, 0f);

        SetSizeCurve(ps, new AnimationCurve(
            new Keyframe(0f, 0.25f),
            new Keyframe(0.25f, 0.7f),
            new Keyframe(1f, 1.35f)));

        return ps;
    }

    private ParticleSystem CreatePoisonBubbles(Transform parent)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "PoisonBubbles", _glowMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 0.65f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.14f);
        main.maxParticles = 16;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.28f;

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;

        velocity.x = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.8f, 1.7f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);

        SetBurst(ps, 9);
        SetFadeCurve(ps, 0.85f, 0f);

        SetSizeCurve(ps, new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.15f, 1f),
            new Keyframe(0.75f, 0.8f),
            new Keyframe(1f, 0f)));

        return ps;
    }

    // =========================================================
    // CORE FLASH
    // =========================================================

    private ParticleSystem CreateCoreFlash(Transform parent, float size, float lifetime)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "CoreFlash", _glowMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = lifetime;
        main.startLifetime = lifetime;
        main.startSpeed = 0f;
        main.startSize = size;
        main.maxParticles = 2;

        SetBurst(ps, 1);
        SetFadeCurve(ps, 1f, 0f);

        SetSizeCurve(ps, new AnimationCurve(
            new Keyframe(0f, 0.15f),
            new Keyframe(0.18f, 1.35f),
            new Keyframe(1f, 0f)));

        return ps;
    }

    // =========================================================
    // SPARKS
    // =========================================================

    private ParticleSystem CreateSparks(
        Transform parent,
        int count,
        float minSpeed,
        float maxSpeed,
        float minSize,
        float maxSize)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "Sparks", _sparkMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 0.35f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.32f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.maxParticles = count + 5;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.08f;

        SetBurst(ps, count);
        SetFadeCurve(ps, 1f, 0f);
        SetSizeCurve(ps, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2f;
        renderer.velocityScale = 0.12f;

        return ps;
    }

    // =========================================================
    // GLINTS
    // =========================================================

    private ParticleSystem CreateGlints(Transform parent, int count, float minSpeed, float maxSpeed)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "Glints", _sparkMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 0.45f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.52f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.13f);
        main.maxParticles = count + 3;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.12f;

        SetBurst(ps, count);
        SetFadeCurve(ps, 1f, 0f);

        SetSizeCurve(ps, new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.12f, 1f),
            new Keyframe(0.65f, 0.8f),
            new Keyframe(1f, 0f)));

        return ps;
    }

    // =========================================================
    // SMOKE
    // =========================================================

    private ParticleSystem CreateSmoke(Transform parent, int count)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "Smoke", _smokeMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 0.7f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.35f, 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
        main.startColor = new Color(0.35f, 0.35f, 0.35f, 0.32f);
        main.maxParticles = count + 3;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        SetBurst(ps, count);
        SetFadeCurve(ps, 0.4f, 0f);

        SetSizeCurve(ps, new AnimationCurve(
            new Keyframe(0f, 0.35f),
            new Keyframe(0.35f, 0.8f),
            new Keyframe(1f, 1.3f)));

        return ps;
    }

    // =========================================================
    // DASH
    // =========================================================

    private ParticleSystem CreateDashStreaks(Transform parent)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "DashStreaks", _sparkMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 0.25f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.16f, 0.3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(-2.2f, -4.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.09f);
        main.maxParticles = 16;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.2f;

        SetBurst(ps, 10);
        SetFadeCurve(ps, 0.9f, 0f);
        SetSizeCurve(ps, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 2.5f;
        renderer.velocityScale = 0.15f;

        return ps;
    }

    private ParticleSystem CreateDashDust(Transform parent)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "DashDust", _smokeMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 0.3f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
        main.startColor = new Color(0.65f, 0.65f, 0.65f, 0.22f);

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        SetBurst(ps, 5);
        SetFadeCurve(ps, 0.3f, 0f);

        return ps;
    }

    // =========================================================
    // LEVEL UP
    // =========================================================

    private ParticleSystem CreateRisingParticles(Transform parent)
    {
        ParticleSystem ps = CreateParticleSystem(parent, "RisingSparkles", _glowMaterial);
        ParticleSystem.MainModule main = ps.main;

        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.15f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.45f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.11f);
        main.maxParticles = 40;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.65f;

        ParticleSystem.VelocityOverLifetimeModule velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.y = new ParticleSystem.MinMaxCurve(1.1f, 2.5f);

        SetBurst(ps, 26);
        SetFadeCurve(ps, 0.9f, 0f);

        SetSizeCurve(ps, new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.15f, 1f),
            new Keyframe(0.8f, 0.7f),
            new Keyframe(1f, 0f)));

        return ps;
    }

    // =========================================================
    // PARTICLE SYSTEM
    // =========================================================

    private ParticleSystem CreateParticleSystem(Transform parent, string objectName, Material material)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = Vector3.zero;

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.gravityModifier = 0f;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = false;

        ParticleSystemRenderer renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        return ps;
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static void SetBurst(ParticleSystem ps, int count)
    {
        ParticleSystem.EmissionModule emission = ps.emission;

        ParticleSystem.Burst[] bursts =
        {
            new(0f, (short)count)
        };

        emission.SetBursts(bursts);
    }

    private static void SetParticleColor(ParticleSystem ps, Color color, float alpha = 1f)
    {
        if (ps == null) return;

        color.a = alpha;

        ParticleSystem.MainModule main = ps.main;
        main.startColor = color;
    }

    private static void SetFadeCurve(ParticleSystem ps, float startAlpha, float endAlpha)
    {
        ParticleSystem.ColorOverLifetimeModule module = ps.colorOverLifetime;
        module.enabled = true;

        Gradient gradient = new();

        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(startAlpha, 0.2f),
                new GradientAlphaKey(endAlpha, 1f)
            });

        module.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    private static void SetSizeCurve(ParticleSystem ps, AnimationCurve curve)
    {
        ParticleSystem.SizeOverLifetimeModule module = ps.sizeOverLifetime;
        module.enabled = true;
        module.size = new ParticleSystem.MinMaxCurve(1f, curve);
    }

    // =========================================================
    // RING
    // =========================================================

    private RingFx CreateRing(Transform parent, string objectName)
    {
        GameObject obj = new(objectName);

        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = Vector3.up * 0.03f;

        LineRenderer line = obj.AddComponent<LineRenderer>();

        line.sharedMaterial = _lineMaterial;
        line.useWorldSpace = false;
        line.loop = true;

        const int segments = 64;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
        }

        line.textureMode = LineTextureMode.Stretch;
        line.numCornerVertices = 2;
        line.numCapVertices = 2;
        line.enabled = false;

        return new RingFx
        {
            Transform = obj.transform,
            Line = line
        };
    }

    // =========================================================
    // RUNTIME RESOURCES
    // =========================================================

    private void CreateRuntimeResources()
    {
        _glowTexture = CreateGlowTexture(64);
        _sparkTexture = CreateSparkTexture(64);

        Shader additive = FindShader(
            "Legacy Shaders/Particles/Additive",
            "Mobile/Particles/Additive",
            "Particles/Additive",
            "Sprites/Default");

        Shader alpha = FindShader(
            "Legacy Shaders/Particles/Alpha Blended",
            "Mobile/Particles/Alpha Blended",
            "Sprites/Default");

        _glowMaterial = CreateMaterial(additive, _glowTexture, "Runtime_Glow");
        _sparkMaterial = CreateMaterial(additive, _sparkTexture, "Runtime_Spark");
        _smokeMaterial = CreateMaterial(alpha, _glowTexture, "Runtime_Smoke");
        _lineMaterial = CreateMaterial(additive, Texture2D.whiteTexture, "Runtime_Ring");
    }

    private static Material CreateMaterial(Shader shader, Texture texture, string materialName)
    {
        Material material = new(shader)
        {
            name = materialName,
            hideFlags = HideFlags.HideAndDontSave,
            mainTexture = texture,
            renderQueue = 3000
        };

        return material;
    }

    private static Shader FindShader(params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            Shader shader = Shader.Find(names[i]);

            if (shader != null)
                return shader;
        }

        return Shader.Find("Sprites/Default");
    }

    // =========================================================
    // GLOW TEXTURE
    // =========================================================

    private static Texture2D CreateGlowTexture(int size)
    {
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false, true)
        {
            name = "Runtime_Glow_Texture",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;

                float distance = Mathf.Sqrt(nx * nx + ny * ny);
                float value = Mathf.Pow(Mathf.Clamp01(1f - distance), 2.2f);

                pixels[y * size + x] = new Color(value, value, value, value);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);

        return texture;
    }

    // =========================================================
    // SPARK TEXTURE
    // =========================================================

    private static Texture2D CreateSparkTexture(int size)
    {
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false, true)
        {
            name = "Runtime_Spark_Texture",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = Mathf.Abs((x + 0.5f) / size * 2f - 1f);
                float ny = Mathf.Abs((y + 0.5f) / size * 2f - 1f);

                float horizontal = Mathf.Exp(-ny * ny * 110f) * Mathf.Pow(1f - nx, 2f);
                float vertical = Mathf.Exp(-nx * nx * 110f) * Mathf.Pow(1f - ny, 2f);
                float radial = Mathf.Clamp01(1f - Mathf.Sqrt(nx * nx + ny * ny) * 2.5f);

                float value = Mathf.Clamp01(Mathf.Max(horizontal, vertical) + radial);

                pixels[y * size + x] = new Color(value, value, value, value);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);

        return texture;
    }

    // =========================================================
    // CLEANUP
    // =========================================================

    private static void DestroyRuntimeObject(Object obj)
    {
        if (obj != null)
            Destroy(obj);
    }
}
