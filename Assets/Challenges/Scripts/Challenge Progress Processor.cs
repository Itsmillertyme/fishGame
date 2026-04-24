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
                controller.AddProgressToChallenge(currentWaterBody.Definition.Id, challenge.Definition.Id, 1);

                Debug.Log($"Challenge Progressed: {challenge.Definition.DisplayName} ({challenge.CurrentProgressValue}/{challenge.Definition.TargetCount})");

                if (challenge.IsCompleted) {
                    Debug.Log($"Challenge Completed: {challenge.Definition.DisplayName}");

                    AwardMoney(controller, currentWaterBody);
                }
            }
        }

        currentWaterBody.RefreshCompletedChallengeCount();
    }

    bool DoesMatch(ChallengeDefinition definition, CatchInfo catchInfo, RodItem rod, ReelItem reel, LureItem lure) {
        if (definition.RequireTrophy && !catchInfo.isTrophy) {
            return false;
        }

        bool baseMatch = false;

        switch (definition.ObjectiveType) {
            case ChallengeObjectiveType.CatchTotalFish:
                baseMatch = true;
                break;

            case ChallengeObjectiveType.CatchSpecies:
                baseMatch = MatchesSpecies(definition, catchInfo);
                break;

            case ChallengeObjectiveType.CatchUsingRod:
                baseMatch = MatchesRod(definition, rod);
                break;

            case ChallengeObjectiveType.CatchUsingReel:
                baseMatch = MatchesReel(definition, reel);
                break;

            case ChallengeObjectiveType.CatchUsingLure:
                baseMatch = MatchesLure(definition, lure);
                break;
        }

        if (!baseMatch) {
            return false;
        }

        if (!MatchesSpecies(definition, catchInfo)) {
            return false;
        }

        if (!MatchesRod(definition, rod)) {
            return false;
        }

        if (!MatchesReel(definition, reel)) {
            return false;
        }

        if (!MatchesLure(definition, lure)) {
            return false;
        }

        return true;
    }

    private bool MatchesSpecies(ChallengeDefinition definition, CatchInfo catchInfo) {
        if (definition.RequiredSpecies == null || definition.RequiredSpecies.Count == 0) {
            return true;
        }

        for (int i = 0; i < definition.RequiredSpecies.Count; i++) {
            if (catchInfo.species == definition.RequiredSpecies[i]) {
                return true;
            }
        }

        return false;
    }

    private bool MatchesRod(ChallengeDefinition definition, RodItem rod) {
        if (definition.RequiredRods == null || definition.RequiredRods.Count == 0) {
            return true;
        }

        for (int i = 0; i < definition.RequiredRods.Count; i++) {
            if (rod == definition.RequiredRods[i]) {
                return true;
            }
        }

        return false;
    }

    private bool MatchesReel(ChallengeDefinition definition, ReelItem reel) {
        if (definition.RequiredReels == null || definition.RequiredReels.Count == 0) {
            return true;
        }

        for (int i = 0; i < definition.RequiredReels.Count; i++) {
            if (reel == definition.RequiredReels[i]) {
                return true;
            }
        }

        return false;
    }

    private bool MatchesLure(ChallengeDefinition definition, LureItem lure) {
        if (definition.RequiredLures == null || definition.RequiredLures.Count == 0) {
            return true;
        }

        for (int i = 0; i < definition.RequiredLures.Count; i++) {
            if (lure == definition.RequiredLures[i]) {
                return true;
            }
        }

        return false;
    }

    private void AwardMoney(GameSessionController controller, WaterBodyRuntimeState waterBody) {
        PlayerDataRuntime player = GameObject.FindFirstObjectByType<PlayerDataRuntime>();

        ProgressionTier tier = waterBody.Definition.ProgressionTier;

        int reward = GetRandomRewardForTier(tier);

        player.AddMoney(reward);
    }

    int GetRandomRewardForTier(ProgressionTier tier) {
        switch (tier) {
            case ProgressionTier.Tier1: return Random.Range(25, 51);
            case ProgressionTier.Tier2: return Random.Range(50, 101);
            case ProgressionTier.Tier3: return Random.Range(100, 176);
            default: return 50;
        }
    }
    #endregion
}