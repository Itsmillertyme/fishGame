using System.Collections.Generic;

[System.Serializable]
public class WaterBodyRuntimeState {
    #region Variables

    WaterBodyDefinition definition;
    bool isUnlocked;
    bool hasBeenVisited;
    List<ChallengeInstance> activeChallenges;
    int completedChallengeCount;

    public WaterBodyDefinition Definition => definition;
    public bool IsUnlocked => isUnlocked;
    public bool HasBeenVisited => hasBeenVisited;
    public IReadOnlyList<ChallengeInstance> ActiveChallenges => activeChallenges;
    public int CompletedChallengeCount => completedChallengeCount;

    #endregion

    #region Utility Methods

    public WaterBodyRuntimeState(WaterBodyDefinition definition, bool isUnlocked, List<ChallengeInstance> activeChallenges) {
        this.definition = definition;
        this.isUnlocked = isUnlocked;
        this.activeChallenges = activeChallenges ?? new List<ChallengeInstance>();
        hasBeenVisited = false;
        RefreshCompletedChallengeCount();
    }

    public void SetUnlocked(bool value) {
        isUnlocked = value;
    }

    public void MarkVisited() {
        hasBeenVisited = true;
    }

    public void RefreshCompletedChallengeCount() {
        completedChallengeCount = 0;

        if (activeChallenges == null) {
            return;
        }

        for (int i = 0; i < activeChallenges.Count; i++) {
            if (activeChallenges[i] != null && activeChallenges[i].IsCompleted) {
                completedChallengeCount++;
            }
        }
    }

    public ChallengeInstance GetChallengeById(string challengeId) {
        if (string.IsNullOrWhiteSpace(challengeId) || activeChallenges == null) {
            return null;
        }

        for (int i = 0; i < activeChallenges.Count; i++) {
            ChallengeInstance challengeInstance = activeChallenges[i];

            if (challengeInstance == null || challengeInstance.Definition == null) {
                continue;
            }

            if (challengeInstance.Definition.Id == challengeId) {
                return challengeInstance;
            }
        }

        return null;
    }

    #endregion
}