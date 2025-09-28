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
    [SerializeField] private BossArea bossArea;

    [Header("Wave Settings")]
    public List<WaveData> waves;
    public int bossWave;                      
    public float timeBetweenWaves = 10f;
    public Transform enemyHolder;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI waveText;

    private int currentWaveIndex = -1;
    private Coroutine waveLoopCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
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
                currentWaveIndex = waves.Count - 1;
            }

            WaveData wave = waves[currentWaveIndex];

            // Boss wave check
            if (currentWaveIndex == bossWave)
            {
                Debug.Log("[Wave] Boss wave starting!");
                BossWave(wave);
                yield break; 
            }

          
            int totalEnemy = 0;
            foreach (var info in wave.enemySpawnInfoList)
            {
                totalEnemy += info.count;
            }

            Debug.Log($"[Wave] Wave {currentWaveIndex + 1} starting with {totalEnemy} enemies.");
            waveText.text = $"Wave {currentWaveIndex + 1}";

            yield return StartCoroutine(SpawnEnemiesInWave(wave));

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

  
    IEnumerator SpawnEnemiesInWave(WaveData wave)
    {
        foreach (var info in wave.enemySpawnInfoList)
        {
            for (int i = 0; i < info.count; i++)
            {
                GameController.instance.SpawnEnemy(info.prefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

  
    public void ResetWaves()
    {
        if (waveLoopCoroutine != null)
            StopCoroutine(waveLoopCoroutine);

      
        foreach (Transform enemy in GameController.instance._enemyHolder)
        {
            Destroy(enemy.gameObject);
        }

        currentWaveIndex = -1;
        waveLoopCoroutine = StartCoroutine(WaveLoop());
    }

  
    public void BossWave(WaveData bossWaveData)
    {
        Character player = Character.instance;
       

        if (player != null && bossArea != null)
        {
           
            bossArea.CreateBossArea(player.transform.position);

            player.transform.position = bossArea.transform.position;
        }

     
        if (bossArea != null)
        {
            Debug.Log("[Wave] Activating Boss Area.");
            bossArea.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[Wave] BossArea instance not found!");
        }



       
        waveText.text = "BOSS WAVE";

        
        StartCoroutine(SpawnEnemiesInWave(bossWaveData));
    }
}
