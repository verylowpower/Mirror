using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _spawnTime = 0f;
    [SerializeField] private float _minSpawnTime = 1f;
    [SerializeField] private float _maxSpawnTime = 3f;
    [SerializeField] private int _spawnCount = 0;
    [SerializeField] private BoxCollider2D colliderArea;
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
        //spawn enemy
        _spawnTime -= Time.deltaTime;
        if (_spawnTime <= 0f)
        {
            Vector2 randomSpawnPos = GetRandomPosition();
            Instantiate(_enemyPrefab, randomSpawnPos, Quaternion.identity);
            _spawnCount++; //track spawned enemies
            SetSpawnTime();
        }
    }

    //spawn enemy
    private void SetSpawnTime()
    {
        _spawnTime = Random.Range(_minSpawnTime, _maxSpawnTime);
    }

    private Vector2 GetRandomPosition() //random pos to spawn
    {
        if (colliderArea == null) return Vector2.zero; // Safety check
        Bounds boxBounds = colliderArea.bounds;
        Vector2 spawnPos;
        do
        {
            float posX = Random.Range(boxBounds.min.x, boxBounds.max.x);
            float posy = Random.Range(boxBounds.min.y, boxBounds.max.y);
            spawnPos = new Vector2(posX, posy);
        } while (CheckInsideCamera(spawnPos)); // enemy can't spawn inside camera area

        return spawnPos;
    }

    private bool CheckInsideCamera(Vector2 positon) //check camera area
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(positon);
        return viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;
    }
}
