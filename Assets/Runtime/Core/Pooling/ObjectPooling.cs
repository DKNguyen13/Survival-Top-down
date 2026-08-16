using System;
using UnityEngine;
using System.Collections.Generic;

public class ObjectPooling : MonoBehaviour
{
    [Serializable]
    private class PoolConfig
    {
        public PoolType type;
        public GameObject prefab;
        [Min(5)] public int poolSize = 10;
    }

    private class Pool
    {
        public GameObject prefab;
        public Transform root;
        public Queue<GameObject> objects = new();
    }

    public static ObjectPooling Instance { get; private set; }
    [SerializeField] private List<PoolConfig> _poolConfigList = new();
    private readonly Dictionary<PoolType, Pool> _poolDict = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePools();
    }

    #region Init data
    private void InitializePools()
    {
        foreach(var config in _poolConfigList)
        {
            if (config.prefab == null) continue;
            CreatePool(config.type, config);
        }
    }
    #endregion

    #region Get/Return
    public GameObject GetFromPool(PoolType type, Vector3 position, Quaternion rotation)
    {
        if (!_poolDict.TryGetValue(type, out Pool pool))
        {
            Debug.LogError($"Pool {type} does not exist!");
            return null;
        }

        GameObject instance;

        if (pool.objects.Count > 0)
        {
            instance = pool.objects.Dequeue();
        }
        else
        {
            instance = CreateInstance(pool);
        }

        instance.transform.SetParent(null);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        return instance;
    }

    public void ReturnToPool(PoolType type, GameObject instance)
    {
        if (!_poolDict.TryGetValue(type, out Pool pool))
        {
            Debug.LogError($"Pool {type} does not exist!");
            Destroy(instance);
            return;
        }

        instance.SetActive(false);
        instance.transform.SetParent(pool.root);
        pool.objects.Enqueue(instance);
    }
    #endregion

    #region Helper
    private void CreatePool(PoolType type, PoolConfig config)
    {
        if (_poolDict.ContainsKey(type)) return;
        GameObject rootObject = new GameObject(type.ToString() + "_Pool");
        rootObject.transform.SetParent(transform);

        Pool pool = new Pool
        {
            prefab = config.prefab,
            root = rootObject.transform
        };

        for(int i = 0; i < config.poolSize; i++)
        {
            GameObject instance = CreateInstance(pool);
            instance.SetActive(false);
            pool.objects.Enqueue(instance);
        }
        _poolDict.Add(type, pool);
    }

    private GameObject CreateInstance(Pool pool)
    {
        GameObject instance = Instantiate(pool.prefab, pool.root);
        return instance;
    }
    #endregion
}