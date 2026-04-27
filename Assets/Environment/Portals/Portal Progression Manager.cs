using UnityEngine;

public class PortalProgressionManager : MonoBehaviour {

    #region Variables
    [SerializeField] PortalScript tier1Portal;
    [SerializeField] PortalScript tier2Portal;
    [SerializeField] PortalScript tier3Portal;

    PlayerDataRuntime player;
    #endregion

    #region Unity Methods
    private void OnEnable() {
        ChallengeProgressProcessor.OnFirstChallengeCompletedForTier += HandleFirstChallengeCompletedForTier;
    }

    private void OnDisable() {
        ChallengeProgressProcessor.OnFirstChallengeCompletedForTier -= HandleFirstChallengeCompletedForTier;
    }

    private void Start() {
        player = FindFirstObjectByType<PlayerDataRuntime>();

        tier1Portal.SetPortalActive(true);
        tier2Portal.SetPortalActive(player.tier2Unlocked);
        tier3Portal.SetPortalActive(player.tier3Unlocked);
    }
    #endregion

    #region Utility Methods
    private void HandleFirstChallengeCompletedForTier(ProgressionTier tier) {
        if (tier == ProgressionTier.Tier1) {
            tier2Portal.SetPortalActive(true);
            player.tier2Unlocked = true;
        }

        if (tier == ProgressionTier.Tier2) {
            tier3Portal.SetPortalActive(true);
            player.tier3Unlocked = true;
        }
    }
    #endregion







}