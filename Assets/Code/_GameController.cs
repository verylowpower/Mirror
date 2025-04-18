using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Rendering;

public class GameController : MonoBehaviour
{
    public bool demo;

    //[SerializeField] private BoxCollider2D colliderArea;

    //player
    public static GameController instance;
    public Transform character;
    CharacterController charScript;
    public CharacterController CharScript { get { return charScript; } }

    //enemy
    public GameObject _enemyPrefab;
    public Transform _enemyHolder;
    public float _spawnTime = 0f;
    //public float _minSpawnTime = 1f;
    public float _spawnTimeCD = 3f;
    //public int _spawnCount = 0;
    public int _enemyCount = 0;
    public int _maxCount = 10000;

    //enemy logic
    Dictionary<int, List<Enemy>> enemyBatch = new(); //store enemy list for handle
    float runLogicTimer = 0f;
    float runLogicTimerCD = 1f;

    //spatial patitioning
    int spatialGroupWidth = 100; //split map to cell to handle
    public int SpatialGroupWidth { get { return spatialGroupWidth; } }
    int spatialGroupHeight = 100;
    public int SpatialGroupHeight { get { return spatialGroupHeight; } }
    int numberOfPartitions = 1000;
    public int NumberOfPartitions { get { return numberOfPartitions; } }
    //border of the map
    int mapWidthMin = -1;
    public int MapWidthMin { get { return mapWidthMin; } }
    int mapWidthMax = -1;
    public int MapWidthMax { get { return mapWidthMax; } }
    int mapHeightMin = -1;
    public int MapHeightMin { get { return mapHeightMin; } }
    int mapHeightMax = -1;
    public int MapHeightMax { get { return mapHeightMax; } }

    //for enemy
    [HideInInspector] public Dictionary<int, HashSet<Enemy>> enemySpatialGroups = new Dictionary<int, HashSet<Enemy>>();
    //for bullet
    [HideInInspector] public Dictionary<int, HashSet<Bullet>> bulletSpatialGroups = new Dictionary<int, HashSet<Bullet>>();

    //exp 
    public GameObject experiencePoint;
    public Transform experiencePointHolder;

    //get spatial group static 
    int Cell_Per_Row_Static;
    int Cell_Per_Col_Static;
    float Cell_Width_Static;
    float Cell_Height_Static;
    int Half_Width_Static;
    int Half_Height_Static;

    //min heap for batch
    /*func: 
        handle enemy in batch
        find batch to spawn enemy
    */
    public class BatchScore : System.IComparable<BatchScore>
    {
        public int BatchId { get; }
        public int Score { get; set; }

        public BatchScore(int batchId, int score)
        {
            BatchId = batchId;
            Score = score; //number of enemy
        }

        public void UpdateScore(int delta)
        {
            Score += delta;
        }

        public int CompareTo(BatchScore other)
        {
            int scoreComparison = Score.CompareTo(other.Score);
            if (scoreComparison == 0)
            {
                //if score equal, compare to batchId
                return BatchId.CompareTo(other.BatchId);
            }
            return scoreComparison;
        }
    }
    //sort batch store list
    SortedSet<BatchScore> batchQueue_Enemy = new();

    //track current score of each batch
    Dictionary<int, BatchScore> batchScoreMap_Enemy = new Dictionary<int, BatchScore>();
    public void AddEnemyToBatch(int batchId, Enemy enemy)
    {
        enemyBatch[batchId].Add(enemy);
    }

    public void UpdateEnemyOnUnitDeath(string option, int batchId)
    {
        if (option == "enemy")
        {
            UpdateEnemyOnDeathRaw(batchQueue_Enemy, batchScoreMap_Enemy, batchId);
        }
    }

    void UpdateEnemyOnDeathRaw(SortedSet<BatchScore> batchQueue, Dictionary<int, BatchScore> batchScoreMap, int batchId)
    {
        if (batchScoreMap.ContainsKey(batchId))
        {
            BatchScore batchScore = batchScoreMap[batchId];
            batchQueue.Remove(batchScore);
            batchScore.UpdateScore(-1);
            batchQueue.Add(batchScore);
           // Debug.Log($"[Batch] Decreased score of batch {batchId} to {batchScore.Score}");
        }
        else
        {
            //Debug.LogError("[Batch] ERROR: Missing batch ID in score map during enemy death.");
        }
    }

    public int GetBestBatch(string option)
    {
        if (option == "enemy")
        {
            return GetBestBatchRaw(batchQueue_Enemy);
        }
        else return -1;
    }

    int GetBestBatchRaw(SortedSet<BatchScore> batchQueue)
    {
        BatchScore leastLoadBatch = batchQueue.Min;

        if (leastLoadBatch == null)
        {
            //Debug.LogError("[Batch] ERROR: leastLoadBatch is null.");
            return 0;
        }
        batchQueue.Remove(leastLoadBatch); //remove old batch
        leastLoadBatch.UpdateScore(1); //add 
        batchQueue.Add(leastLoadBatch); //update
       // Debug.Log($"[Batch] Chose batch {leastLoadBatch.BatchId} with updated score {leastLoadBatch.Score}");
        return leastLoadBatch.BatchId;
    }

    void IntitalizeBatches()
    {
        //Debug.Log("[Init] Initializing enemy batches...");

        for (int i = 0; i < 50; i++)
        {
            BatchScore batchScore = new(i, 0);

            enemyBatch.Add(i, new List<Enemy>()); //create enemy list of each batch
            batchScoreMap_Enemy.Add(i, batchScore); //update score for each batch
            batchQueue_Enemy.Add(batchScore);//add batch have least score to leastLoadBatch for spawn enemy
            //Debug.Log($"[Init] Created batch {i}");
        }
    }


    void Awake()
    {
        instance = this;

    }

    void Start()
    {
        //static get spatial group
        Cell_Per_Col_Static = (int)Mathf.Sqrt(numberOfPartitions);
        Cell_Per_Row_Static = Cell_Per_Col_Static;
        Cell_Height_Static = spatialGroupHeight / Cell_Per_Col_Static;
        Cell_Width_Static = spatialGroupWidth / Cell_Per_Row_Static;
        Half_Height_Static = spatialGroupHeight / 2;
        Half_Width_Static = spatialGroupWidth / 2;

        charScript = character.GetComponent<CharacterController>();

        IntitalizeBatches();

        for (int i = 0; i < numberOfPartitions; i++)
        {
            enemySpatialGroups.Add(i, new HashSet<Enemy>());
            bulletSpatialGroups.Add(i, new HashSet<Bullet>());

        }

        int initEnemySpawn = demo ? 5 : 20;
        _maxCount = demo ? 100 : 1000;
        _enemyCount = demo ? 5 : 20;
        for (int i = 0; i < initEnemySpawn; i++)
        {

            SpawnEnemy();

        }

        mapWidthMin = -spatialGroupWidth / 2;
        mapWidthMax = spatialGroupWidth / 2;
        mapHeightMin = -spatialGroupHeight / 2;
        mapHeightMax = spatialGroupHeight / 2;


    }


    void FixedUpdate()
    {
        if (instance.character == null) return;

        runLogicTimer += Time.deltaTime;

        if (runLogicTimer > runLogicTimerCD)
        {
            RunOnceASecondLogicForAllBullet();
            runLogicTimer = 0f;
            return;
        }

        SpawnEnemies();
        int batchId = (int)(runLogicTimer * 50) % 50; // <-- giới hạn trong 0–49
        RunBatchLogic(batchId);

    }

    void RunOnceASecondLogicForAllBullet()
    {
        foreach (Bullet bullet in bulletSpatialGroups.SelectMany(x => x.Value).ToList())
        {
            bullet.OnceASecondLogic();
        }
    }

    private void RunBatchLogic(int batchId)
    {
        //Debug.Log($"[Batch] Running logic for batch {batchId}");

        if (!enemyBatch.ContainsKey(batchId)) return;

        foreach (Enemy enemy in enemyBatch[batchId])
        {
            //Debug.Log($"[Enemy] Running logic for enemy in batch {batchId}");
            if (enemy) enemy.RunLogic();
        }
    }


    void SpawnEnemies()
    {
        int initEnemySpawn = demo ? 5 : 20;
        _spawnTime += Time.deltaTime;

        if (_spawnTime > _spawnTimeCD && _enemyHolder.childCount < _maxCount)
        {
            for (int i = 0; i < initEnemySpawn; i++)
            {
                SpawnEnemy();
                _enemyCount++;
            }
            _spawnTime = 0;
        }
    }


    void SpawnEnemy()
    {
        int batchToAdd = GetBestBatch("enemy");

        if (batchToAdd == -1 || !enemyBatch.ContainsKey(batchToAdd))
        {
            //Debug.LogError("[Spawn] ERROR: Invalid batch ID received.");
            return;
        }

        int charQuadrant = GetSpatialGroupDynamic(character.position.x, character.position.y, spatialGroupHeight, spatialGroupWidth, numberOfPartitions);

        List<int> expandedSpatialGroup = EnemyHelper.GetExpandedSpatialGroups(charQuadrant, 25);

        expandedSpatialGroup.Remove(charQuadrant);

        int randomSpatialGroup = expandedSpatialGroup[Random.Range(0, expandedSpatialGroup.Count)];
        if (expandedSpatialGroup.Count == 0)
        {
            //Debug.LogWarning("[Spawn] WARNING: No valid expanded spatial groups.");
            return;
        }

        Vector2 centerOfSpatialGroup = GetPatitionCenterDynamic(randomSpatialGroup, spatialGroupWidth, spatialGroupHeight, numberOfPartitions);

        float sizeOfOneSpatialGroup = spatialGroupWidth / 5;
        float valX = Random.Range(centerOfSpatialGroup.x - sizeOfOneSpatialGroup / 2,
                                 centerOfSpatialGroup.x + sizeOfOneSpatialGroup / 2);
        float valY = Random.Range(centerOfSpatialGroup.y - sizeOfOneSpatialGroup / 2,
                                 centerOfSpatialGroup.y + sizeOfOneSpatialGroup / 2);

        GameObject enemyGO = Instantiate(_enemyPrefab, _enemyHolder);

        enemyGO.transform.position = new Vector3(valX, valY, 0);
        enemyGO.transform.parent = _enemyHolder;

        Enemy enemyScript = enemyGO.GetComponent<Enemy>();

        int spatialGroup = GetSpatialGroup(enemyGO.transform.position.x, enemyGO.transform.position.y);
        enemyScript.spatialGroup = spatialGroup;

        AddToSpatialGroup(spatialGroup, enemyScript);

        enemyScript.BatchID = batchToAdd;
        enemyBatch[batchToAdd].Add(enemyScript);
       // Debug.Log($"[Spawn] Spawned enemy in batch {batchToAdd} at group {spatialGroup} ({valX:F2}, {valY:F2})");

    }

    public int GetSpatialGroup(float xPos, float yPos)
    {
        return GetSpatialGroupDynamic(xPos, yPos, spatialGroupWidth, spatialGroupHeight, numberOfPartitions);
    }

    public int GetSpatialGroupStatic(float xPos, float yPos)
    {
        float adjustedX = xPos + Half_Width_Static;
        float adjustedY = yPos + Half_Height_Static;

        int xIndex = (int)(adjustedX / Cell_Width_Static);
        int yIndex = (int)(adjustedY / Cell_Height_Static);

        return xIndex + yIndex * Cell_Per_Row_Static;

    }

    public int GetSpatialGroupDynamic(float xPos, float yPos, float mapWidth, float mapHeight, int totalPartitions)
    {
        //calculate number of cells
        int cellsPerRow = (int)Mathf.Sqrt(totalPartitions);
        int cellsPerCol = cellsPerRow;
        //calculate size of cell
        float cellWidth = mapWidth / cellsPerCol;
        float cellHeight = mapHeight / cellsPerRow;

        float adjustedX = xPos + (mapWidth / 2);
        float adjustedY = yPos + (mapHeight / 2);

        int xIndex = (int)(adjustedX / cellWidth);
        int yIndex = (int)(adjustedY / cellHeight);

        xIndex = Mathf.Clamp(xIndex, 0, cellsPerRow - 1);
        yIndex = Mathf.Clamp(yIndex, 0, cellsPerCol - 1);

        return xIndex + yIndex * cellsPerRow;
    }

    Vector2 GetPatitionCenterDynamic(int partition, float mapWidth, float mapHeight, int totalPartitions)
    {
        int cellPerRow = (int)Mathf.Sqrt(totalPartitions);
        int cellPerCol = cellPerRow;

        float cellWidth = mapWidth / cellPerCol;
        float cellHeight = mapHeight / cellPerRow;

        int rowIndex = partition / cellPerRow; // row index of partition 
        int colIndex = partition % cellPerCol;

        /*example: 0 1 2
                   3 4 5
                   6 7 8 
                   partition = 7 rowIndex = 7/3 = 2 colIndex = 7%3 = 2 with 1 
        because the spatial is square
        */

        float centerX = (colIndex + 0.5f) * cellWidth - (mapWidth / 2);
        float centerY = (rowIndex + 0.5f) * cellHeight - (mapHeight / 2);

        return new Vector2(centerX, centerY);
    }

    public void AddToSpatialGroup(int spatialGroupId, Enemy enemy)
    {
        enemySpatialGroups[spatialGroupId].Add(enemy);
       // Debug.Log($"[Spatial] Added enemy to group {spatialGroupId}");
    }

    public void RemoveFromSpatialGroup(int spatialGroupId, Enemy enemy)
    {
        enemySpatialGroups[spatialGroupId].Remove(enemy);
       // Debug.Log($"[Spatial] Removed enemy from group {spatialGroupId}");
    }

    public void DropExpPoint(Vector3 position, int amount)
    {
        GameObject expGO = Instantiate(experiencePoint, position, Quaternion.identity);
        expGO.transform.parent = experiencePointHolder;
    }




    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.cyan;

    //     int cellPerRow = (int)Mathf.Sqrt(numberOfPartitions);
    //     int cellPerCol = cellPerRow;

    //     float cellWidth = spatialGroupWidth / (float)cellPerCol;
    //     float cellHeight = spatialGroupHeight / (float)cellPerRow;

    //     float halfMapWidth = spatialGroupWidth / 2f;
    //     float halfMapHeight = spatialGroupHeight / 2f;

    //     for (int row = 0; row < cellPerRow; row++)
    //     {
    //         for (int col = 0; col < cellPerCol; col++)
    //         {
    //             float centerX = (col + 0.5f) * cellWidth - halfMapWidth;
    //             float centerY = (row + 0.5f) * cellHeight - halfMapHeight;

    //             Vector3 center = new Vector3(centerX, centerY, 0);
    //             Vector3 size = new Vector3(cellWidth, cellHeight, 0);

    //             Gizmos.DrawWireCube(center, size);
    //         }
    //     }
    // }


    // public void DropExp()
    // {

    // }

    // void Update()
    // {
    //     //spawn enemy
    //     _spawnTime -= Time.deltaTime;
    //     if (_spawnTime <= 0f)
    //     {
    //         Vector2 randomSpawnPos = GetRandomPosition();
    //         Instantiate(_enemyPrefab, randomSpawnPos, Quaternion.identity);
    //         _spawnCount++; //track spawned enemies
    //         SetSpawnTime();
    //     }
    // }

    // //spawn enemy
    // private void SetSpawnTime()
    // {
    //     _spawnTime = Random.Range(_minSpawnTime, _maxSpawnTime);
    // }

    // private Vector2 GetRandomPosition() //random pos to spawn
    // {
    //     if (colliderArea == null) return Vector2.zero; // Safety check
    //     Bounds boxBounds = colliderArea.bounds;
    //     Vector2 spawnPos;
    //     do
    //     {
    //         float posX = Random.Range(boxBounds.min.x, boxBounds.max.x);
    //         float posy = Random.Range(boxBounds.min.y, boxBounds.max.y);
    //         spawnPos = new Vector2(posX, posy);
    //     } while (CheckInsideCamera(spawnPos)); // enemy can't spawn inside camera area

    //     return spawnPos;
    // }

    // private bool CheckInsideCamera(Vector2 positon) //check camera area
    // {
    //     Vector3 viewportPos = Camera.main.WorldToViewportPoint(positon);
    //     return viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;
    // }
}