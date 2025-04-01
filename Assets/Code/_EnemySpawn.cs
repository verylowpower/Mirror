using UnityEngine;

public class _EnemySpawn : MonoBehaviour
{

    [SerializeField] GameObject _enemyPrefap;
    [SerializeField] private float _spawnTime = 0f;
    [SerializeField] private float _minSpawnTime = 0f;
    [SerializeField] private float _maxSpawnTime = 0f;
    [SerializeField] private int _spawnCount = 0;
    public BoxCollider2D colliderArea;

    void Awake()
    {
        SetSpawnTime();
    }


    void Update()
    {
        var randomSpawnPos = GetRandomPosition();
        _spawnTime -= Time.deltaTime;
        if (_spawnTime <= 0f)
        {
            Instantiate(_enemyPrefap, randomSpawnPos, Quaternion.identity);
            SetSpawnTime();
        }
    }   
    
    private void SetSpawnTime()
    {
        _spawnTime = Random.Range(_minSpawnTime, _maxSpawnTime);
    }

    private Vector2 GetRandomPosition()
    {
        var basePos = colliderArea.transform.position;
        var size = colliderArea.size;
        var posX = basePos.x + Random.Range(-size.x, size.x);
        var posY = basePos.x + Random.Range(-size.y, size.y);

        var spawnPos = new Vector2(posX, posY);
        return spawnPos;
    }
}
