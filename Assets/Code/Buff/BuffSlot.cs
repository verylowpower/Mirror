using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffSlot : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI decsText;
    public Image iconImg;

    private string buffID;
    private System.Action<string> onClick;

    public void Setup(Buff buff, System.Action<string> callback)
    {
        buffID = buff.ID;
        nameText.text = buff.Name;
        decsText.text = buff.Decription;

        if (iconImg != null && buff.Icon != null)
        {
            iconImg.sprite = buff.Icon;
        }

        onClick = callback;
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(OnClick);

    }


    public void OnClick()
    {
        //Debug.Log("Selected: " + buffID);
        onClick?.Invoke(buffID);
       
    }
}
