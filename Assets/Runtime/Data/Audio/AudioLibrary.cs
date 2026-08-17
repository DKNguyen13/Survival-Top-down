using System;
using UnityEngine;

public enum SfxId
{
    PlayerShoot,
    PlayerBulletHit,
    BombPlace,
    BombExplosion,
    Dash,
    MeleeAttack,
    PoisonShoot,
    PoisonHit,
    PlayerHurt,
    EnemyDeath,
    LevelUp,
    GameOver,
}

public enum BgmId
{
    Gameplay
}

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Survival/Data/Audio Library")]
public sealed class AudioLibrary : ScriptableObject
{
    [Serializable]
    public sealed class SfxEntry
    {
        public SfxId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 1.5f)] public float minPitch = 0.95f;
        [Range(0.5f, 1.5f)] public float maxPitch = 1.05f;
        [Range(0f, 1f)] public float spatialBlend = 0.7f;
        [Min(0f)] public float minInterval = 0.04f;
    }

    [Serializable]
    public sealed class BgmEntry
    {
        public BgmId id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.55f;
    }

    [SerializeField] private SfxEntry[] _sfx = Array.Empty<SfxEntry>();
    [SerializeField] private BgmEntry[] _bgm = Array.Empty<BgmEntry>();

    public bool TryGetSfx(SfxId id, out SfxEntry entry)
    {
        for (int i = 0; i < _sfx.Length; i++)
        {
            if (_sfx[i] != null && _sfx[i].id == id)
            {
                entry = _sfx[i];
                return entry.clip != null;
            }
        }

        entry = null;
        return false;
    }

    public bool TryGetBgm(BgmId id, out BgmEntry entry)
    {
        for (int i = 0; i < _bgm.Length; i++)
        {
            if (_bgm[i] != null && _bgm[i].id == id)
            {
                entry = _bgm[i];
                return entry.clip != null;
            }
        }

        entry = null;
        return false;
    }
}