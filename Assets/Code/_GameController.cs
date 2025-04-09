using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Rendering;

public class GameController : MonoBehaviour
{
    public bool demo;

    [SerializeField] private BoxCollider2D colliderArea;

    //player
    public static GameController instance;
    public Transform character;
    CharacterController charScript;
    public CharacterController CharScript { get { return charScript; } }

    //enemy
    public GameObject _enemyPrefab;
    public Transform _enemyHolder;
    public float _spawnTime = 0f;
    public float _minSpawnTime = 1f;
    public float _maxSpawnTime = 3f;
    public int _spawnCount = 0;
    public int _maxCount = 100000;

    //enemy logic
    Dictionary<int, List<Enemy>> enemyBatch = new(); //store enemy list for handle
    float runLogicTimer = 0f;
    float runLogicTimerCD = 1f;

    //spatital patitioning
    int spatitalGroupWidth = 100; //split map to cell to handle
    public int SpatitalGroupWidth { get { return spatitalGroupWidth; } }
    int spatitalGroupHeight = 100;
    public int SpatitalGroupHeight { get { return spatitalGroupHeight; } }
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
    [HideInInspector] public Dictionary<int, HashSet<Enemy>> enemySpatitalGroups = new Dictionary<int, HashSet<Enemy>>();
    //[HideInInspector] public Dictionary<int, HashSet<Bullet>> bulletSpatitalGroups = new Dictionary<int, HashSet<Bullet>>();

    //exp 
    public GameObject experiencePoint;
    public Transform experiencePointHolder;

    //get spatital group static 
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
        }
        else
        {
            Debug.Log("BUG AT UPDATE ENEMY ON DEATH RAW");
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
            Debug.Log("BUG GET BEST BATCH RAW, NULL LEAST LOAD BATCH");
            return 0;
        }
        batchQueue.Remove(leastLoadBatch); //remove old batch
        leastLoadBatch.UpdateScore(1); //add 
        batchQueue.Add(leastLoadBatch); //update

        return leastLoadBatch.BatchId;
    }

    void IntitalizeBatches()
    {
        for (int i = 0; i < 50; i++)
        {
            BatchScore batchScore = new(i, 0);

            enemyBatch.Add(i, new List<Enemy>()); //create enemy list of each batch
            batchScoreMap_Enemy.Add(i, batchScore); //update score for each batch
            batchQueue_Enemy.Add(batchScore);//add batch have least score to leastLoadBatch for spawn enemy
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
        Cell_Height_Static = spatitalGroupHeight / Cell_Per_Col_Static;
        Cell_Width_Static = spatitalGroupWidth / Cell_Per_Row_Static;
        Half_Height_Static = spatitalGroupHeight / 2;
        Half_Width_Static = spatitalGroupWidth / 2;

        charScript = character.GetComponent<CharacterController>();

        IntitalizeBatches();

        for (int i = 0; i < numberOfPartitions; i++)
        {
            enemySpatitalGroups.Add(i, new HashSet<Enemy>());
            //bulletSpatitalGroups.Add(i, new HashSet<Bullet>());

        }

        int initEnemySpawn = demo ? 100 : 1000000;
        _maxCount = demo ? 100 : 1000000;
        for (int i = 0; i < initEnemySpawn; i++)
        {
            SpawnEnemy();
        }

        _spawnTime = 0f;

    }

    void SpawnEnemy()
    {
        int batchToAdd = GetBestBatch("enemy");

        int charQuadrant = GetSpatitalGroupDynamic(character.position.x, character.position.y, spatitalGroupHeight, spatitalGroupWidth, 25);

        List<int> expandedSpatitalGroup = EnemyHelper.GetExpandedSpatialGroups(charQuadrant, 0);

        expandedSpatitalGroup.Remove(charQuadrant);

        int randomSpatitalGroup = expandedSpatitalGroup[Random.Range(0, expandedSpatitalGroup.Count)];

        Vector2 centerOfSpatitalGroup = GetPatitionCenterDynamic(randomSpatitalGroup, spatitalGroupWidth, spatitalGroupHeight, 25);

        float sizeOfOneSpatitalGroup = spatitalGroupWidth / 5;
        float valX = Random.Range(centerOfSpatitalGroup.x - sizeOfOneSpatitalGroup / 2,
                                 centerOfSpatitalGroup.y + sizeOfOneSpatitalGroup / 2);
        float valY = Random.Range(centerOfSpatitalGroup.y - sizeOfOneSpatitalGroup / 2,
                                 centerOfSpatitalGroup.y + sizeOfOneSpatitalGroup / 2);

        GameObject enemyGO = Instantiate(_enemyPrefab, _enemyHolder);

        enemyGO.transform.position = new Vector3(valX, valY, 0);
        enemyGO.transform.parent = _enemyHolder;

        Enemy enemyScript = enemyGO.GetComponent<Enemy>();

        int spatialGroup = GetSpatitalGroup(enemyGO.transform.position.x, enemyGO.transform.position.y);
        enemyScript.spatialGroup = spatialGroup;

        AddToSpatitalGroup(spatialGroup, enemyScript);

        enemyScript.BatchID = batchToAdd;
        enemyBatch[batchToAdd].Add(enemyScript);

    }

    public int GetSpatitalGroup(float xPos, float yPos)
    {
        return GetSpatitalGroupDynamic(xPos, yPos, spatitalGroupWidth, spatitalGroupHeight, numberOfPartitions);
    }

    public int GetSpatitalGroupStatic(float xPos, float yPos)
    {
        float adjustedX = xPos + Half_Width_Static;
        float adjustedY = yPos + Half_Height_Static;

        int xIndex = (int)(adjustedX / Cell_Width_Static);
        int yIndex = (int)(adjustedY / Cell_Height_Static);

        return xIndex + yIndex * Cell_Per_Row_Static;

    }

    public int GetSpatitalGroupDynamic(float xPos, float yPos, float mapWidth, float mapHeight, int totalPartitions)
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
        because the spatital is square
        */

        float centerX = (colIndex + 0.5f) * cellWidth - (mapWidth / 2);
        float centerY = (rowIndex + 0.5f) * cellHeight - (mapHeight / 2);

        return new Vector2(centerX, centerY);
    }

    public void AddToSpatitalGroup(int spatialGroupId, Enemy enemy)
    {
        enemySpatitalGroups[spatialGroupId].Add(enemy);
    }

    public void RemoveFromSpatitalGroup(int spatialGroupId, Enemy enemy)
    {
        enemySpatitalGroups[spatialGroupId].Remove(enemy);
    }



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