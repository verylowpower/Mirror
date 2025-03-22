using UnityEngine;

public class _EnemySpawn : MonoBehaviour
{

    [SerializeField] GameObject _enemyPrefap;
    [SerializeField] private float _spawnTime = 0f;
    [SerializeField] private float _minSpawnTime = 0f;
    [SerializeField] private float _maxSpawnTime = 0f;

    void Awake()
    {
        SetSpawnTime();
    }


    void Update()
    {
        _spawnTime -= Time.deltaTime;
        if (_spawnTime <= 0f)
        {
            Instantiate(_enemyPrefap, transform.position, Quaternion.identity);
            SetSpawnTime();
        }
    }

    private void SetSpawnTime()
    {
        _spawnTime = Random.Range(_minSpawnTime, _maxSpawnTime);
    }

}
