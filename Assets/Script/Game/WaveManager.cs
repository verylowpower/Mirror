using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject prefab;
    public int count;
}

[System.Serializable]
public class WaveData
{
    public List<EnemySpawnInfo> enemySpawnInfoList;
    public float spawnInterval = 0.5f;
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;
    
    [Header("Wave Settings")]
    public List<WaveData> waves;
    public Transform enemyHolder;
    public float timeBetweenWaves = 10f;
    private int currentWaveIndex = -1;
    [SerializeField] private TextMeshProUGUI waveText;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(WaveLoop());
        ResetWaves();
    }

    IEnumerator WaveLoop()
    {
        while (true)
        {
            currentWaveIndex++;

            if (currentWaveIndex >= waves.Count)
            {
                Debug.Log("[Wave] All predefined waves completed. Restarting last wave...");
                currentWaveIndex = waves.Count - 1; // giữ wave cuối
            }

            WaveData wave = waves[currentWaveIndex];

            int totalEnemy = 0;
            foreach (var info in wave.enemySpawnInfoList)
            {
                totalEnemy += info.count;
            }

            Debug.Log($"[Wave] Wave {currentWaveIndex + 1} starting with {totalEnemy} enemies.");
            StartCoroutine(SpawnEnemiesInWave(wave));

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator SpawnEnemiesInWave(WaveData wave)
    {
        waveText.text = $"Wave {currentWaveIndex + 1}";

        foreach (var info in wave.enemySpawnInfoList)
        {
            for (int i = 0; i < info.count; i++)
            {
                GameController.instance.SpawnEnemy(info.prefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

    // void SpawnEnemy(List<GameObject> enemyPrefabs)
    // {
    //     if (enemyPrefabs == null || enemyPrefabs.Count == 0) return;

    //     int index = Random.Range(0, enemyPrefabs.Count);
    //     GameObject prefab = enemyPrefabs[index];

    //     GameController.instance.SpawnEnemy(prefab);
    // }

    public void ResetWaves()
    {
        StopAllCoroutines();
        currentWaveIndex = -1;

        foreach (Transform enemy in GameController.instance._enemyHolder)
        {
            Destroy(enemy.gameObject);
        }

        StartCoroutine(WaveLoop());
    }
}
