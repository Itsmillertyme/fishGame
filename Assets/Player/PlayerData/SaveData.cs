using System.IO;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    [SerializeField] private PlayerDataRuntime playerDataRuntime;
    [SerializeField] private TackleBox tackleBox;
    [SerializeField] private GameSessionController gameSessionController;

    private const string FILE_NAME = "playerdata.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, FILE_NAME);

    private void Awake()
    {
        Load();
    }

    public void Save()
    {
        if (playerDataRuntime == null)
        {
            Debug.LogWarning("SaveData: PlayerDataRuntime is missing.");
            return;
        }

        if (gameSessionController != null && gameSessionController.CurrentRun != null)
        {
            playerDataRuntime.SaveChallengeStates(gameSessionController.CurrentRun);
        }

        var data = playerDataRuntime.Data;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"Saved player data to {SavePath}");
    }

    public void Load()
    {
        if (playerDataRuntime == null)
        {
            Debug.LogWarning("SaveData: PlayerDataRuntime is missing.");
            return;
        }

        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            var loaded = JsonUtility.FromJson<PlayerData>(json);

            playerDataRuntime.InitializeFromLoaded(loaded);
            playerDataRuntime.LoadEquippedCardsToTackleBox(tackleBox);

            Debug.Log("Loaded player data");
        }
        else
        {
            playerDataRuntime.InitializeFromLoaded(null);
            Debug.Log("No player data found, starting new");
        }
    }

    public void ApplyLoadedChallengeStateToCurrentRun()
    {
        if (playerDataRuntime == null || gameSessionController == null)
        {
            return;
        }

        if (gameSessionController.CurrentRun == null)
        {
            Debug.LogWarning("SaveData: No current run to apply challenge state to.");
            return;
        }

        playerDataRuntime.ApplyChallengeStates(gameSessionController.CurrentRun);
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