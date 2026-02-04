using UnityEngine;

public class FishEncounterTester : MonoBehaviour {

    #region Variables
    [Header("References")]
    [SerializeField] FishingMinigameController controller;
    [SerializeField] FishSpawnTable spawnTable;

    [Header("Debug / Inputs")]
    [SerializeField] RodType rodType = RodType.Spinning;

    FishRoll lastRoll;
    bool hasLastRoll;
    #endregion

    #region Unity Methods
    void OnEnable() {
        controller.OnMinigameEnded += HandleMinigameEnded;
    }

    void OnDisable() {
        controller.OnMinigameEnded -= HandleMinigameEnded;
    }
    #endregion


    #region Utility Methods
    [ContextMenu("Roll Fish And Start Minigame")]
    public void RollFishAndStartMinigame() {
        if (controller == null || spawnTable == null) return;

        lastRoll = spawnTable.Roll();
        hasLastRoll = lastRoll.config != null;

        if (!hasLastRoll) return;

        MinigameDifficulty difficulty = PickDifficultyFromSize(lastRoll.size01, lastRoll.isTrophy);
        bool extendedFight = lastRoll.isTrophy || lastRoll.size01 >= 0.85f;

        if (rodType == RodType.Spinning) {
            controller.StartSpinningRodMinigame(lastRoll.config, lastRoll.size01, difficulty, extendedFight);
        }
        else {
            controller.StartCastingRodMinigame(lastRoll.config, lastRoll.size01, difficulty, extendedFight);
        }

        Debug.Log($"Rolled fish: {lastRoll.config.displayName} | size01={lastRoll.size01:0.00} | trophy={lastRoll.isTrophy} | diff={difficulty} | extended={extendedFight}");
    }

    [ContextMenu("Reset Minigame")]
    public void ResetMinigame() {
        if (controller == null) return;

        controller.StopMinigame(MinigameEndReason.Cancelled);

        hasLastRoll = false;
        lastRoll = default;

        Debug.Log("Minigame reset.");
    }

    MinigameDifficulty PickDifficultyFromSize(float size01, bool isTrophy) {
        if (isTrophy) return MinigameDifficulty.Hard;
        if (size01 < 0.40f) return MinigameDifficulty.Easy;
        if (size01 < 0.80f) return MinigameDifficulty.Medium;
        return MinigameDifficulty.Hard;
    }

    void HandleMinigameEnded(MinigameResult r) {
        if (r.reason == MinigameEndReason.Success) {
            // award fish
        }
        else {
            // fish got away
        }
    }

}
#endregion

#region Enums
public enum RodType {
    Spinning,
    Casting
}
#endregion
