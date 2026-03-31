using UnityEngine;

public class ChallengeProgressProcessor {
    #region Utility Methods

    public void ProcessFishCaught(CatchInfo catchInfo, RodItem rod, ReelItem reel, LureItem lure) {
        GameSessionController controller = GameSessionController.Instance;

        if (controller == null || controller.CurrentRun == null) {
            Debug.LogWarning("ChallengeProgressProcessor: No active run.");
            return;
        }

        WaterBodyRuntimeState currentWaterBody = controller.GetCurrentWaterBody();

        if (currentWaterBody == null || currentWaterBody.Definition == null) {
            Debug.LogWarning("ChallengeProgressProcessor: No current water body.");
            return;
        }

        var challenges = currentWaterBody.ActiveChallenges;

        for (int i = 0; i < challenges.Count; i++) {
            ChallengeInstance challenge = challenges[i];

            if (challenge == null || challenge.Definition == null) {
                continue;
            }

            if (challenge.IsCompleted) {
                continue;
            }

            if (DoesMatch(challenge.Definition, catchInfo, rod, reel, lure)) {
                challenge.AddProgress(1);

                Debug.Log($"Challenge Progressed: {challenge.Definition.DisplayName} ({challenge.CurrentProgressValue}/{challenge.Definition.TargetCount})");

                if (challenge.IsCompleted) {
                    Debug.Log($"Challenge Completed: {challenge.Definition.DisplayName}");
                }
            }
        }

        currentWaterBody.RefreshCompletedChallengeCount();

        ChallengeUIController challengeUI = Object.FindFirstObjectByType<ChallengeUIController>();

        if (challengeUI != null) {
            challengeUI.RefreshAllChallengeUI();
        }
    }

    private bool DoesMatch(ChallengeDefinition definition, CatchInfo catchInfo, RodItem rod, ReelItem reel, LureItem lure) {
        switch (definition.ObjectiveType) {
            case ChallengeObjectiveType.CatchTotalFish:
                return true;

            case ChallengeObjectiveType.CatchSpecies:
                return catchInfo.species == definition.RequiredSpecies;

            case ChallengeObjectiveType.CatchUsingRod:
                return rod == definition.RequiredRod;

            case ChallengeObjectiveType.CatchUsingReel:
                return reel == definition.RequiredReel;

            case ChallengeObjectiveType.CatchUsingLure:
                return lure == definition.RequiredLure;
        }

        return false;
    }

    #endregion
}