using UnityEngine;
using Unity.Services.LevelPlay;

/// <summary>
/// Initializes Unity LevelPlay SDK (Ads Mediation 9.3) on app startup.
/// Creates and loads rewarded, interstitial, and banner ad objects.
/// Exposes public references for AdsManager to use.
/// Runs once per app session (attach to DontDestroyOnLoad scene).
/// </summary>
public class AdsInitialization : MonoBehaviour
{
    [Header("From unity.com/levelplay dashboard")]
    [SerializeField] private string appKey = "YOUR_APP_KEY_HERE";        // Unique app ID from LevelPlay dashboard
    [SerializeField] private string rewardedId = "YOUR_REWARDED_ID";     // Placement ID for rewarded videos (upgrades)
    [SerializeField] private string interstitialId = "YOUR_INTERSTITIAL_ID";  // Placement ID for skippable ads (~2 min)
    [SerializeField] private string bannerId = "YOUR_BANNER_ID";         // Placement ID for bottom banner

    /// <summary>
    /// Public references to ad objects.
    /// AdsManager drags this script and accesses these fields.
    /// Null until SDK initializes successfully.
    /// </summary>
    public LevelPlayRewardedAd rewardedAd;
    public LevelPlayInterstitialAd interstitialAd;
    public LevelPlayBannerAd bannerAd;

    void Start()
    {
        /*
         * Subscribe to SDK init events BEFORE calling Init().
         * OnInitSuccess: SDK ready, create/load ads.
         * OnInitFailed: Network/config error, log and retry later.
         */
        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;

        // Kick off SDK initialization (downloads network configs, ~1-2 seconds)
        LevelPlay.Init(appKey);
    }

    /// <summary>
    /// SDK callback when initialization succeeds.
    /// Creates ad objects with placement IDs, starts loading (downloads/cache ads).
    /// Banner uses 1-arg constructor (defaults: BANNER size, bottom position).
    /// LoadAd() runs asynchronously—use IsAdReady() before ShowAd().
    /// </summary>
    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay 9.3 initialized successfully");

        // Instantiate ad objects (SDK allocates native resources)
        rewardedAd = new LevelPlayRewardedAd(rewardedId);           // Rewarded: 30s video ? FishCoins/boosters
        interstitialAd = new LevelPlayInterstitialAd(interstitialId);  // Interstitial: Skippable 15-30s ? session break
        bannerAd = new LevelPlayBannerAd(bannerId);                 // Banner: Persistent bottom ad ? always visible

        // Queue ad downloads (background, multiple in parallel)
        rewardedAd.LoadAd();
        interstitialAd.LoadAd();
        bannerAd.LoadAd();

        // Ready in ~1-5 seconds; AdsManager checks IsAdReady() before showing
    }

    /// <summary>
    /// SDK callback on init failure (bad app key, no network, etc.).
    /// Logs error but doesn't crash—retry possible on next app open.
    /// Dashboard shows detailed diagnostics.
    /// </summary>
    private void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"LevelPlay Init failed: {error.ErrorCode} - {error.ErrorMessage}");
        // Optional: retry logic or fallback to IAP-only
    }

    void OnDestroy()
    {
        /*
         * Unsubscribe events to prevent memory leaks.
         * Events are global—leftover subscriptions cause crashes on scene reload.
         * Ad objects cleaned up by AdsManager.DestroyAd().
         */
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed -= OnInitFailed;
    }
}
