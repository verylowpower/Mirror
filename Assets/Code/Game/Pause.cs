using Unity.VisualScripting;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public static Pause instance;
    public GameObject pauseMenu;
    public bool isPaused;

    void Start()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0 : 1;
            pauseMenu.SetActive(isPaused);
        }
    }
}
