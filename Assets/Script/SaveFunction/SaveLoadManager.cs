using System.IO;
using UnityEngine;

public static class SaveLoadManager
{
    private static string filePath = Application.persistentDataPath + "/save.json";

    public static void Save(GameProgress data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("[SaveLoad] Game saved at: " + filePath);
    }

    public static GameProgress Load()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            GameProgress data = JsonUtility.FromJson<GameProgress>(json);
            Debug.Log("[SaveLoad] Game loaded: " + json);
            return data;
        }
        else
        {
            Debug.Log("[SaveLoad] No save file found.");
            return null;
        }
    }

    public static bool HasSave()
    {
        return File.Exists(filePath);
    }

    public static void DeleteSave()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("[SaveLoad] Save file deleted.");
        }
    }
}
