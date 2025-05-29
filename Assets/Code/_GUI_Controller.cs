using UnityEngine;
using TMPro;

public class GUI_Controller : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI pointText;
    private float time;

    void Start()
    {
        Character.instance.OnLevelChanged += UpdateLevelText;
        GameController.instance.TimeChange += UpdateGameTime;
        GameController.instance.KilledEnemy += UpdatePointText;
        UpdateLevelText(); // Gọi 1 lần ban đầu
        UpdatePointText();
    }

    private void UpdateLevelText()
    {
        levelText.text = "Level: " + Character.instance.Level.ToString();
        Debug.Log("UPDATE LEVEL TEXT");
    }

    // void OnDestroy()
    // {
    //     Character.instance.OnLevelChanged -= UpdateLevelText;
    //     GameController.instance.TimeChange -= UpdateGameTime;
    //     GameController.instance.KilledEnemy -= UpdatePointText;
    // }


    private void UpdateGameTime()
    {
        time = GameController.instance.inGameTime;
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
        Debug.Log("UPDATE TIME TEXT");
    }

    private void UpdatePointText()
    {
        pointText.text = "Score: " + GameController.instance.enemyKilled.ToString();
    }

}
