using UnityEngine;
using Unity.Services.LevelPlay;

public class AdsInitializer : MonoBehaviour
{
    [SerializeField] private string appKey = "YOUR_APP_KEY_HERE"; // From LevelPlay dashboard

    void Start()
    {
        // Register events
        LevelPlay.OnInitSuccess += OnSdkInitSuccess;
        LevelPlay.OnInitFailed += OnSdkInitFailed;

        // Init SDK (call once on app start)
        LevelPlay.Init(appKey);

        // Enable test suite
        LevelPlay.SetMetaData("is_test_suite", "enable");
    }

    private void OnSdkInitSuccess()
    {
        Debug.Log("LevelPlay SDK initialized successfully");

        // Init specific ad units (recommended for perf)
        LevelPlay.InitAdUnits(LevelPlayAdUnits.REWARDED_VIDEO | LevelPlayAdUnits.INTERSTITIAL | LevelPlayAdUnits.BANNER);
    }

    private void OnSdkInitFailed(LevelPlayError error)
    {
        Debug.LogError($"LevelPlay init failed: {error.Message}");
    }

    void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnSdkInitSuccess;
        LevelPlay.OnInitFailed -= OnSdkInitFailed;
    }
}
