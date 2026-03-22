using UnityEngine;
using Unity.Services.LevelPlay;

/// <summary>
/// Manages all ad showing logic for fishGame monetization.
/// References pre-created ad objects from AdsInitializer.
/// Handles timing, daily limits, and callbacks for rewards.
/// </summary>
public class AdsManager : MonoBehaviour
{
    [Header("Drag AdsInitializer from scene here")]
    [SerializeField] private AdsInitialization initializer;  // Holds the actual LevelPlay ad objects

    /// <summary>
    /// Cooldown timer for interstitial ads (~2 minutes between shows as per design doc).
    /// Prevents spam during gameplay.
    /// </summary>
    private float interstitialCooldown = 0f;

    /// <summary>
    /// Stores callback to run when rewarded ad completes (e.g., add FishCoins).
    /// Set before showing ad, invoked on success.
    /// </summary>
    private System.Action rewardedCallback;

    void Start()
    {
        /*
         * Attach event listeners to ad objects created in AdsInitializer.
         * These fire automatically when ad lifecycle events happen (load, show, reward, close).
         * 
         * OnAdRewarded: Fires when player watches full rewarded video.
         * Lambda for OnAdClosed: Reloads ad automatically after close (keeps queue ready).
         */
        initializer.rewardedAd.OnAdRewarded += OnAdRewarded;  // Triggers reward grant
        initializer.rewardedAd.OnAdClosed += (info) => initializer.rewardedAd.LoadAd();  // Auto-reload
        initializer.interstitialAd.OnAdClosed += (info) => initializer.interstitialAd.LoadAd();  // Auto-reload
    }

    /// <summary>
    /// Public method to show rewarded ad from anywhere in game (e.g., challenge screen).
    /// Checks: ad ready + under daily limit (3x as per design doc).
    /// Sets callback, shows ad, handles failure by reloading.
    /// </summary>
    public void ShowRewarded(System.Action onReward)
    {
        // IsAdReady() = SDK confirms ad is downloaded and cached
        if (initializer.rewardedAd.IsAdReady() && DailyRewardedCount() < 3)
        {
            rewardedCallback = onReward;  // Store user callback (e.g., AddFishCoins(100))
            initializer.rewardedAd.ShowAd();  // Triggers SDK ad display
        }
        else
        {
            // Not ready or limit hit ? reload for next time
            initializer.rewardedAd.LoadAd();
        }
    }

    /// <summary>
    /// SDK callback when rewarded video is fully watched.
    /// Delegate signature: LevelPlayAdInfo first (ad metadata), LevelPlayReward second (reward data).
    /// Grants user's callback + increments daily counter.
    /// </summary>
    private void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        // Log reward details (e.g., "coins x100")
        Debug.Log($"Reward granted: {reward.Name} x{reward.Amount}");

        rewardedCallback?.Invoke();  // Run user's code (FishCoins, booster, etc.)
        rewardedCallback = null;     // Clear reference

        IncrementDailyRewarded();    // Track 3x/day limit
    }

    /// <summary>
    /// Public method for interstitial (skippable ad every ~2 mins during gameplay).
    /// Checks: ad ready + cooldown elapsed.
    /// Auto-reloads after show via OnAdClosed listener.
    /// </summary>
    public void ShowInterstitial()
    {
        // Double-check: SDK ready + 120s since last interstitial
        if (initializer.interstitialAd.IsAdReady() && Time.time > interstitialCooldown)
        {
            initializer.interstitialAd.ShowAd();
            interstitialCooldown = Time.time + 120f;  // Reset 2-minute cooldown
        }
        // Silently fails if not ready (no spam)
    }

    /// <summary>
    /// Simple banner methods.
    /// ShowAd(): Displays persistent bottom banner (as per design doc).
    /// DestroyAd(): Removes banner (e.g., during minigames).
    /// </summary>
    public void ShowBanner() => initializer.bannerAd.ShowAd();
    public void HideBanner() => initializer.bannerAd.DestroyAd();

    /// <summary>
    /// PlayerPrefs-based daily counter for rewarded ads (resets at midnight).
    /// Prevents abuse beyond design doc limit (3x/day).
    /// </summary>
    private int DailyRewardedCount() => PlayerPrefs.GetInt("RewardedDaily_" + System.DateTime.Now.Date.ToString(), 0);

    /// <summary>
    /// Increments daily rewarded counter.
    /// Key format ensures reset each day.
    /// </summary>
    private void IncrementDailyRewarded() =>
        PlayerPrefs.SetInt("RewardedDaily_" + System.DateTime.Now.Date.ToString(), DailyRewardedCount() + 1);

    void OnDestroy()
    {
        /*
         * Clean shutdown: DestroyAd() releases native resources.
         * Prevents memory leaks when scene unloads or app quits.
         * Safe to call even if null.
         */
        initializer.rewardedAd?.DestroyAd();
        initializer.interstitialAd?.DestroyAd();
        initializer.bannerAd?.DestroyAd();
    }
}
