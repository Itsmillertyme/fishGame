using UnityEngine;
using Unity.Services.LevelPlay;

public class AdsManager : MonoBehaviour
{
    // Placement names from dashboard (e.g., "RewardedVideo", "Interstitial", "Banner")
    [SerializeField] private string rewardedPlacement = "RewardedVideo";
    [SerializeField] private string interstitialPlacement = "Interstitial";
    [SerializeField] private string bannerPlacement = "Banner";

    private bool rewardedReady = false;
    private bool interstitialReady = false;
    private float interstitialCooldown = 0f; // For ~2 min spacing

    void Start()
    {
        // Register ad events
        LevelPlay.OnRewardedVideoAdShowResult += OnRewardedShowResult;
        LevelPlay.OnRewardedVideoAdReady += (placement) => { if (placement == rewardedPlacement) rewardedReady = true; };
        LevelPlay.OnRewardedVideoAdShowFailed += (placement, error) => { rewardedReady = false; LoadRewarded(); };

        LevelPlay.OnInterstitialAdReady += (placement) => { if (placement == interstitialPlacement) interstitialReady = true; };
        LevelPlay.OnInterstitialAdShowFailed += (placement, error) => { interstitialReady = false; LoadInterstitial(); };

        // Load initial ads
        LoadRewarded();
        LoadInterstitial();
        LoadBanner();
    }

    // Rewarded (for upgrades, 3x/day limit)
    public void ShowRewarded(System.Action onReward)
    {
        if (rewardedReady && DailyRewardedCount() < 3)
        {
            rewardedCallback = onReward;
            LevelPlay.ShowRewardedVideo(rewardedPlacement);
        }
        else
        {
            LoadRewarded(); // Auto-reload
        }
    }

    private void LoadRewarded() => LevelPlay.LoadRewardedVideo(rewardedPlacement);

    private System.Action rewardedCallback;
    private void OnRewardedShowResult(bool didWatch, LevelPlayPlacementInfo placementInfo)
    {
        if (didWatch && rewardedCallback != null)
        {
            rewardedCallback(); // Grant FishCoins or booster
            IncrementDailyRewarded();
        }
        rewardedReady = false;
        LoadRewarded();
    }

    // Interstitial (every couple mins)
    public bool CanShowInterstitial() => interstitialReady && Time.time > interstitialCooldown;

    public void ShowInterstitial()
    {
        if (CanShowInterstitial())
        {
            LevelPlay.ShowInterstitial(interstitialPlacement);
            interstitialCooldown = Time.time + 120f; // 2 min cooldown
        }
    }

    private void LoadInterstitial() => LevelPlay.LoadInterstitial(interstitialPlacement);

    // Banner (bottom screen)
    public void ShowBanner()
    {
        LevelPlay.LoadBanner(bannerPlacement, LevelPlayBannerSize.BANNER, LevelPlayBannerPosition.BOTTOM_CENTER);
        LevelPlay.ShowBanner(bannerPlacement);
    }

    public void HideBanner() => LevelPlay.HideBanner(bannerPlacement);

    // Utils (PlayerPrefs for simplicity; use cloud saves later)
    private int DailyRewardedCount()
    {
        string key = "RewardedDaily_" + System.DateTime.Now.Date.ToString();
        return PlayerPrefs.GetInt(key, 0);
    }

    private void IncrementDailyRewarded() => PlayerPrefs.SetInt("RewardedDaily_" + System.DateTime.Now.Date.ToString(), DailyRewardedCount() + 1);

    void OnDestroy()
    {
        LevelPlay.OnRewardedVideoAdShowResult -= OnRewardedShowResult;
        // Unregister other events...
    }
}
