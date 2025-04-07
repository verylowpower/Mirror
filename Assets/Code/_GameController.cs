using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.Rendering;

public class GameController : MonoBehaviour
{


    [SerializeField] private BoxCollider2D colliderArea;

    //player
    public static GameController instance;
    public Transform player;
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
    int numberOfPatition = 1000;
    public int numberOfPatitions { get { return numberOfPatition; } }
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