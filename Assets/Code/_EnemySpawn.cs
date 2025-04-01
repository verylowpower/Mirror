using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _spawnTime = 0f;
    [SerializeField] private float _minSpawnTime = 1f; // Set sensible default values
    [SerializeField] private float _maxSpawnTime = 3f;
    [SerializeField] private int _spawnCount = 0;
    [SerializeField] private BoxCollider2D colliderArea; // Ensure it's assigned

    void Awake()
    {
        if (colliderArea == null)
        {
            Debug.LogError("ColliderArea is not assigned! Please assign it in the Inspector.");
            enabled = false;
            return;
        }

        SetSpawnTime();
    }

    void Update()
    {
        _spawnTime -= Time.deltaTime;
        if (_spawnTime <= 0f)
        {
            Vector2 randomSpawnPos = GetRandomPosition();
            Instantiate(_enemyPrefab, randomSpawnPos, Quaternion.identity);
            _spawnCount++; // Keep track of spawned enemies
            SetSpawnTime();
        }
    }

    private void SetSpawnTime()
    {
        _spawnTime = Random.Range(_minSpawnTime, _maxSpawnTime);
    }

    private Vector2 GetRandomPosition()
    {
        if (colliderArea == null) return Vector2.zero; // Safety check
        Bounds boxBounds = colliderArea.bounds;
        Vector2 spawnPos;
        do
        {
            float posX = Random.Range(boxBounds.min.x, boxBounds.max.x);
            float posy = Random.Range(boxBounds.min.y, boxBounds.max.y);
            spawnPos = new Vector2(posX, posy);
        } while (CheckInsideCamera(spawnPos));

        return spawnPos;
    }

    private bool CheckInsideCamera(Vector2 positon)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(positon);
        return viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;
    }
}
