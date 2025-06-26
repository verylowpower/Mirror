using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathHandler : MonoBehaviour
{
    public static DeathHandler instance;

    [SerializeField] private GameObject deathScreen;

    private void Awake()
    {
        instance = this;
    }

    public void HandlePlayerDeath()
    {
        deathScreen.SetActive(true);
        StartCoroutine(LoadMenuAfterDelay());
    }

    private IEnumerator LoadMenuAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadSceneAsync(0);
    }
}
