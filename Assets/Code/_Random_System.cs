using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RandomSystem : MonoBehaviour
{
    public static RandomSystem instance;
    public Buff_GUI buff_UI;

    void Awake()
    {
        instance = this;
    }

    public void RandomBuff()
    {
        Time.timeScale = 0;
        // Lọc các buff đủ điều kiện
        List<string> availableBuffIDs = new();

        foreach (var kvp in BuffLibrary.AllBuffs)
        {
            string buffID = kvp.Key;
            Buff buff = kvp.Value;

            // Nếu buff chưa được nhận
            if (!Character.instance.unlockedBuffs.Contains(buffID))
            {
                // Và không yêu cầu hoặc yêu cầu đã được mở khóa
                if (string.IsNullOrEmpty(buff.RequirementBuffID) ||
                    Character.instance.unlockedBuffs.Contains(buff.RequirementBuffID))
                {
                    availableBuffIDs.Add(buffID);
                }
            }
        }

        // Nếu không còn buff hợp lệ → không hiện
        if (availableBuffIDs.Count == 0)
        {
            Debug.Log("No valid buffs available.");
            Character.instance.buffUIActive = false;
            return;
        }

        // Random 3 buff từ danh sách đã lọc
        List<string> selected = new();
        for (int i = 0; i < 3 && availableBuffIDs.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, availableBuffIDs.Count);
            string randomID = availableBuffIDs[index];
            availableBuffIDs.RemoveAt(index);
            selected.Add(randomID);

        }

        // Hiển thị UI chọn buff
        buff_UI.ShowBuffs(selected.ToArray(), selectedBuffID =>
        {
            OnBuffSelected(selectedBuffID);
        });
    }


    private void OnBuffSelected(string selectedBuffID)
    {
        if (BuffLibrary.AllBuffs.TryGetValue(selectedBuffID, out Buff buff))
        {
            // Áp dụng hiệu ứng buff
            buff.ApplyEffect?.Invoke();

            // Đánh dấu là đã nhận
            Character.instance.unlockedBuffs.Add(selectedBuffID);

            Debug.Log($"Buff selected and applied: {selectedBuffID}");
        }

        buff_UI.HideAll();
        Character.instance.buffUIActive = false;
        Time.timeScale = 1;
    }

}
