using UnityEngine;

public class TackleBox : MonoBehaviour {

    #region Variables
    [Header("Defaults")]
    [SerializeField] DefaultLoadout defaultLoadout;

    [Header("Equipped (Runtime)")]
    [SerializeField] RodItem equippedRod;
    [SerializeField] ReelItem equippedReel;
    [SerializeField] LureItem equippedLure;
    #endregion

    #region Unity Methods
    void Awake() {
        if (equippedRod == null || equippedReel == null || equippedLure == null) {
            ResetToDefaults();
        }
    }
    #endregion

    #region Utility Methods
    public void ResetToDefaults() {
        if (defaultLoadout == null) return;

        equippedRod = defaultLoadout.startingRod;
        equippedReel = defaultLoadout.startingReel;
        equippedLure = defaultLoadout.startingLure;
    }

    public ReelType GetEquippedSetupType() {
        if (equippedReel == null) return ReelType.Spinning;
        return equippedReel.reelType;
    }

    public TackleModifiers GetCurrentTackleModifiers() {
        TackleModifiers tackleMods = TackleModifiers.Default();

        if (equippedLure != null) {
            tackleMods.biteDelayAddSeconds += equippedLure.biteDelayAddSeconds;
            tackleMods.biteDelayMultiplier *= equippedLure.biteDelayMultiplier;
        }

        if (equippedRod != null) {
            tackleMods.castingLandGainMultiplier *= equippedRod.landGainMultiplier;
            tackleMods.castingLandLossMultiplier *= equippedRod.castingLandLossMultiplier;
        }

        if (equippedReel != null) {
            if (equippedReel.reelType == ReelType.Spinning) {
                tackleMods.spinningSlackGainMultiplier *= equippedReel.slackGainMultiplier;
                tackleMods.spinningSlackDecayMultiplier *= equippedReel.slackDecayMultiplier;
            }
            else {
                tackleMods.castingHeatGainMultiplier *= equippedReel.heatGainMultiplier;
                tackleMods.castingHeatDecayMultiplier *= equippedReel.heatDecayMultiplier;
            }
        }

        return tackleMods;
    }

    public void EquipRod(RodItem rod) { equippedRod = rod; }

    public void EquipReel(ReelItem reel) { equippedReel = reel; }

    public void EquipLure(LureItem lure) { equippedLure = lure; }

    #endregion
}

#region Structs
public struct TackleModifiers {
    // Bite phase
    public float biteDelayAddSeconds;
    public float biteDelayMultiplier;

    // Spinning
    public float spinningLandGainMultiplier;
    public float spinningSlackGainMultiplier;
    public float spinningSlackDecayMultiplier;

    // Casting
    public float castingLandGainMultiplier;
    public float castingLandLossMultiplier;
    public float castingHeatGainMultiplier;
    public float castingHeatDecayMultiplier;

    public static TackleModifiers Default() {
        TackleModifiers tackleMods = new TackleModifiers();
        tackleMods.biteDelayAddSeconds = 0f;
        tackleMods.biteDelayMultiplier = 1f;

        tackleMods.spinningLandGainMultiplier = 1f;
        tackleMods.spinningSlackGainMultiplier = 1f;
        tackleMods.spinningSlackDecayMultiplier = 1f;

        tackleMods.castingLandGainMultiplier = 1f;
        tackleMods.castingLandLossMultiplier = 1f;
        tackleMods.castingHeatGainMultiplier = 1f;
        tackleMods.castingHeatDecayMultiplier = 1f;

        return tackleMods;
    }
}
#endregion