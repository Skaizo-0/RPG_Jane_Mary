using UnityEngine;

public class GameRepository
{
    private const string SaveKey = "PlayerSave";

    public void Save(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public PlayerData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return null;
        string json = PlayerPrefs.GetString(SaveKey);
        return JsonUtility.FromJson<PlayerData>(json);
    }
}