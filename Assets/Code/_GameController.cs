using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//use to control enemy spawn and remove enemy
public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _spawnTime = 0f;
    [SerializeField] private float _minSpawnTime = 1f;
    [SerializeField] private float _maxSpawnTime = 3f;
    [SerializeField] private int _spawnCount = 0;
    [SerializeField] private BoxCollider2D colliderArea;

    public static GameController instance;

    // Spatial Partitioning
    int spatialGroupWidth = 100;
    public int SpatialGroupWidth { get { return spatialGroupWidth; } }
    int spatialGroupHeight = 100;
    public int SpatialGroupHeight { get { return spatialGroupHeight; } }
    int numberOfPartition = 10000;
    public int NumberOfPartition { get { return numberOfPartition; } }

    // Enemy groups in spatial partition
    [HideInInspector] public Dictionary<int, HashSet<Enemy>> enemySpatialGroups = new();

    // Enemy batch management (Optional)
    public Dictionary<int, List<Enemy>> enemyBatches = new();

    void Awake()
    {
        if (colliderArea == null)
        {
            Debug.LogError("ColliderArea is not assigned! Please assign it in the Inspector.");
            enabled = false;
            return;
        }

        SetSpawnTime();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Spawn enemy
        _spawnTime -= Time.deltaTime;
        if (_spawnTime <= 0f)
        {
            SpawnEnemy();
            SetSpawnTime();
        }
    }

    // Spawn enemy at random position
    private void SpawnEnemy()
    {
        Vector2 spawnPosition = GetRandomPosition();
        GameObject enemyGO = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
        _spawnCount++;

        Enemy enemyScript = enemyGO.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            int spatialGroup = GetSpatialGroup(spawnPosition.x, spawnPosition.y);
            enemyScript.spatialGroup = spatialGroup;
            AddToSpatialGroup(spatialGroup, enemyScript);

            int batchID = GetBestBatch();
            enemyScript.BatchID = batchID;
            if (!enemyBatches.ContainsKey(batchID))
            {
                enemyBatches[batchID] = new List<Enemy>();
            }
            enemyBatches[batchID].Add(enemyScript);
        }
    }

    // Set spawn time
    private void SetSpawnTime()
    {
        _spawnTime = Random.Range(_minSpawnTime, _maxSpawnTime);
    }

    // Get random spawn position within collider area
    private Vector2 GetRandomPosition()
    {
        if (colliderArea == null) return Vector2.zero; // Safety check
        Bounds boxBounds = colliderArea.bounds;
        Vector2 spawnPos;
        do
        {
            float posX = Random.Range(boxBounds.min.x, boxBounds.max.x);
            float posY = Random.Range(boxBounds.min.y, boxBounds.max.y);
            spawnPos = new Vector2(posX, posY);
        } while (CheckInsideCamera(spawnPos)); // enemy can't spawn inside camera area

        return spawnPos;
    }

    // Check if the position is inside camera view
    private bool CheckInsideCamera(Vector2 position)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(position);
        return viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;
    }

    // Add enemy to spatial group
    private void AddToSpatialGroup(int spatialGroup, Enemy enemy)
    {
        if (!enemySpatialGroups.ContainsKey(spatialGroup))
        {
            enemySpatialGroups[spatialGroup] = new HashSet<Enemy>();
        }
        enemySpatialGroups[spatialGroup].Add(enemy);
    }

    // Get spatial group based on position
    public int GetSpatialGroup(float x, float y)
    {
        int width = SpatialGroupWidth;
        int height = SpatialGroupHeight;
        return (int)(x / width) + (int)(y / height) * width;
    }

    // Get best batch ID (example function, can be adjusted to your logic)
    private int GetBestBatch()
    {
        return 0; // Example: Return a batch ID based on custom logic
    }

    // Remove enemy from spatial group (when destroyed)
    public void RemoveEnemy(Enemy enemy)
    {
        if (enemySpatialGroups.TryGetValue(enemy.spatialGroup, out var group))
        {
            group.Remove(enemy);
            if (group.Count == 0)
            {
                enemySpatialGroups.Remove(enemy.spatialGroup);
            }
        }
    }
}
