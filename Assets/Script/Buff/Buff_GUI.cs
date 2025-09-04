using UnityEngine;
using TMPro;
using System;

public class Buff_GUI : MonoBehaviour
{
    public BuffSlot[] buffSlots;

    void Start()
    {
        HideAll();
    }

    public void ShowBuffs(string[] buffIDs, Action<string> onBuffSelected)
    {
        for (int i = 0; i < buffSlots.Length; i++)
        {
            if (i < buffIDs.Length && BuffLibrary.AllBuffs.TryGetValue(buffIDs[i], out Buff buff))
            {
                buffSlots[i].gameObject.SetActive(true);
                buffSlots[i].Setup(buff, onBuffSelected);

            }
            else
            {
                //buffSlots[i].text = "";
                buffSlots[i].gameObject.SetActive(false);
            }
        }
    }
    public void HideAll()
    {
        foreach (var slot in buffSlots)
        {
            slot.gameObject.SetActive(false);
        }
    }
}
