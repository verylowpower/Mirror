using UnityEngine;
using TMPro;

public class GUI_Controller : MonoBehaviour
{
    public TextMeshProUGUI levelText;

    void Start()
    {
        Character.instance.OnLevelChanged += UpdateLevelText;
        UpdateLevelText(); // Gọi 1 lần ban đầu
    }

    private void UpdateLevelText()
    {
        levelText.text = "Level: " + Character.instance.Level.ToString();
        Debug.Log("UPDATE TEXT");
    }
}
