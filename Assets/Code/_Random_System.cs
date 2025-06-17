using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RandomSystem : MonoBehaviour
{
    public static RandomSystem instance;
    public Buff_GUI buff_UI;

    void Start()
    {

    }

    void Awake()
    {
        instance = this;
    }

    public void RandomBuff()
    {
        List<string> buffIDs = new(BuffLibrary.AllBuffs.Keys);
        List<string> selected = new();

        if (buffIDs.Count > 0)
        {
            for (int i = 0; i < 3 && buffIDs.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, buffIDs.Count);
                string randomID = buffIDs[index];
                buffIDs.RemoveAt(index);
                selected.Add(randomID);
            }
            // Pass callback for selection
            buff_UI.ShowBuffs(selected.ToArray(), selectedBuffID =>
            {
                OnBuffSelected(selectedBuffID, selected);
            });
            Time.timeScale = 0;
        }
    }

    private void OnBuffSelected(string selectedBuffID, List<string> selectedOption)
    {
        //Debug.Log("Selected Buff: " + selectedBuff);

        if (BuffLibrary.AllBuffs.TryGetValue(selectedBuffID, out Buff buff))
        {
            buff.ApplyEffect?.Invoke();
            Debug.Log($"Apply buff: {buff.Name}");
        }

        foreach (string id in selectedOption)
        {
            BuffLibrary.AllBuffs.Remove(selectedBuffID);

            Debug.Log("Remove: " + selectedBuffID);
        }

        buff_UI.HideAll(); // Hide the buff UI
        Character.instance.buffUIActive = false; // Allow future buffs
    }


}
