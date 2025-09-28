using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Gọi khi nhấn nút "New Game" hoặc "Start"
    public void StartButton()
    {
        SaveLoadManager.DeleteSave();

        SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1f;
    }

    public void ContinueButton()
    {
        GameProgress data = SaveLoadManager.Load();
        if (data != null)
        {
            SceneManager.LoadSceneAsync(data.currentLevel).completed += (op) =>
            {
                Character.instance.transform.position = data.playerPosition;
                Character.instance._curHealth = data.playerHealth;   
            };

            Time.timeScale = 1f;
        }
        else
        {
            Debug.Log("[Menu] No save data found, starting new game.");
            StartButton();
        }
    }

    public void MenuButton()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
    }

    public void ResumeButton()
    {
        SceneManager.UnloadSceneAsync("Menu");
        Pause.instance.isPaused = false;
        Pause.instance.pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
