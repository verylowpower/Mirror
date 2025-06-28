using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class WaveData
{
    public int enemyCount;
    public float spawnInterval = 0.5f;
    public GameObject enemyPrefab;
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

            Debug.Log($"[Wave] Wave {currentWaveIndex + 1} starting with {wave.enemyCount} enemies.");
            StartCoroutine(SpawnEnemiesInWave(wave));

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator SpawnEnemiesInWave(WaveData wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            waveText.text = $"Wave {currentWaveIndex + 1}";
            SpawnEnemy(wave.enemyPrefab);
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        GameController.instance.SpawnEnemy(enemyPrefab);

    }

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
