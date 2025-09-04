using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void StartButton()
    {
        SceneManager.LoadSceneAsync(1);
        Time.timeScale = 1f;
    }

    public void MenuButton()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
        //Pause.instance.isPaused = false;
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
