using System;
using UnityEngine;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private PlayerController _player;
    [SerializeField] private float _spawnRadius = 10f;

    private int _aliveEnemies;
    private int _wave;
    private readonly Dictionary<EnemyHealth, PoolType> _spawnedEnemyTypes = new();
    public event Action<int, int> WaveChanged;

    private void Start()
    {
        SpawnNextWave();
    }

    private void SpawnNextWave()
    {
        _wave++;
        GetEnemyCounts(out int meleeCount, out int rangedCount);
        _aliveEnemies = meleeCount + rangedCount;
        WaveChanged?.Invoke(_wave, _aliveEnemies);

        for (int i = 0; i < meleeCount; i++)
        {
            Spawn(PoolType.MeleeEnemy);
        }

        for (int i = 0; i < rangedCount; i++)
        {
            Spawn(PoolType.RangedEnemy);
        }
    }

    private void Spawn(PoolType type)
    {
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _spawnRadius;

        // Get enemy from pool
        GameObject enemyGO = ObjectPooling.Instance.GetFromPool(type, _player.transform.position + offset, Quaternion.identity);
        EnemyController enemy = enemyGO.GetComponent<EnemyController>();

        _spawnedEnemyTypes[enemy.Health] = type;
        enemy.Health.Died -= HandleEnemyDied;
        enemy.Health.Died += HandleEnemyDied;
        enemy.Initialize(_player.transform);
    }

    private void HandleEnemyDied(EnemyHealth health)
    {
        health.Died -= HandleEnemyDied;
        EnemyController enemy = health.GetComponent<EnemyController>();
        _player.GetComponent<PlayerProgression>().AddExperience(enemy.Config.ExperienceReward);

        if (_spawnedEnemyTypes.Remove(health, out PoolType poolType))
        {
            ObjectPooling.Instance.ReturnToPool(poolType, health.gameObject);
        }

        _aliveEnemies--;
        WaveChanged?.Invoke(_wave, _aliveEnemies);
        if (_aliveEnemies == 0) Invoke(nameof(SpawnNextWave), 1.5f);
    }

    private void GetEnemyCounts(out int meleeCount, out int rangedCount)
    {
        if (_wave == 1)
        {
            meleeCount = 1;
            rangedCount = 0;
            return;
        }

        if (_wave == 2)
        {
            meleeCount = 1;
            rangedCount = 1;
            return;
        }

        meleeCount = UnityEngine.Random.Range(3, 5);
        rangedCount = UnityEngine.Random.Range(1, 3);
    }

    // Getter, Setter
    public int Wave => _wave;
    public int AliveEnemies => _aliveEnemies;
}