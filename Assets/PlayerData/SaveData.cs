using System.IO;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    [SerializeField] private PlayerDataRuntime playerDataRuntime;

    private const string FILE_NAME = "playerdata.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, FILE_NAME);

    private void Awake()
    {
        Load();
    }

    public void Save()
    {
        var data = playerDataRuntime.Data;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Saved player data to {SavePath}");
    }

    public void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            var loaded = JsonUtility.FromJson<PlayerData>(json);
            playerDataRuntime.InitializeFromLoaded(loaded);
            Debug.Log("Loaded player data");
        }
        else
        {
            playerDataRuntime.InitializeFromLoaded(null);
            Debug.Log("No player data found, starting new");
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) Save();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus) Save();
    }
}
