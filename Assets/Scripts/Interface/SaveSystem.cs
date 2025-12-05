using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string savePath = Application.persistentDataPath + "/save.json";

    public static void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("SaveSystem: Saved game to " + savePath);
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("SaveSystem: No save file found at " + savePath);
            return null;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("SaveSystem: Loaded save from " + savePath);
        return data;
    }

    public static bool HasSave()
    {
        return File.Exists(savePath);
    }
}
