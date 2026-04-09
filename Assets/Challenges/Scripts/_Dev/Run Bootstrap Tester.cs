using UnityEngine;

public class RunBootstrapTester : MonoBehaviour {
    #region Variables

    [Header("References")]
    [SerializeField] GameSessionController gameSessionController;
    [SerializeField] TackleBox playerTackleBox;
    [SerializeField] FishingInteractor fishingInteractor;
    [SerializeField] ChallengeUIController challengeUIController;

    [Header("Debug")]
    [SerializeField] bool autoStartRunOnAwake = true;
    [SerializeField] bool logRunOnStart = true;
    [SerializeField] bool markCurrentBodyVisited = true;

    private ChallengeProgressProcessor progressProcessor = new ChallengeProgressProcessor();

    #endregion

    #region Unity Methods

    private void Awake() {
        if (gameSessionController == null) {
            gameSessionController = GameSessionController.Instance;
        }

        if (!autoStartRunOnAwake) {
            return;
        }

        StartTestRun();
    }

    private void Start() {
        if (!logRunOnStart) {
            return;
        }

        LogCurrentRunState();
    }

    #endregion

    #region Utility Methods

    [ContextMenu("Start Test Run")]
    public void StartTestRun() {
        if (gameSessionController == null) {
            Debug.LogError("RunBootstrapTester: No GameSessionController reference found.");
            return;
        }

        gameSessionController.StartNewRun();

        if (challengeUIController != null) {
            challengeUIController.BuildChallengeUI();
        }

        if (markCurrentBodyVisited) {
            WaterBodyRuntimeState currentWaterBody = gameSessionController.GetCurrentWaterBody();

            if (currentWaterBody != null) {
                currentWaterBody.MarkVisited();
            }
        }

        ValidateGeneratedChallenges();

        Debug.Log("RunBootstrapTester: Test run started.");
    }

    [ContextMenu("Log Current Run State")]
    public void LogCurrentRunState() {
        if (gameSessionController == null) {
            Debug.LogError("RunBootstrapTester: No GameSessionController reference found.");
            return;
        }

        if (gameSessionController.CurrentRun == null) {
            Debug.LogWarning("RunBootstrapTester: No active run exists.");
            return;
        }

        RunState currentRun = gameSessionController.CurrentRun;

        Debug.Log("========================================");
        Debug.Log("RUN BOOTSTRAP TEST");
        Debug.Log("Run Id: " + currentRun.RunId);

        if (currentRun.CurrentWaterBody != null && currentRun.CurrentWaterBody.Definition != null) {
            Debug.Log("Current Water Body: " + currentRun.CurrentWaterBody.Definition.DisplayName);
        }
        else {
            Debug.Log("Current Water Body: None");
        }

        Debug.Log("Total Water Bodies: " + currentRun.WaterBodies.Count);

        for (int i = 0; i < currentRun.WaterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = currentRun.WaterBodies[i];

            if (waterBodyState == null || waterBodyState.Definition == null) {
                Debug.LogWarning("Water Body [" + i + "] is null.");
                continue;
            }

            Debug.Log("----------------------------------------");
            Debug.Log(
                "Water Body [" + i + "]\n" +
                "Id: " + waterBodyState.Definition.Id + "\n" +
                "Name: " + waterBodyState.Definition.DisplayName + "\n" +
                "Scene: " + waterBodyState.Definition.SceneName + "\n" +
                "Tier: " + waterBodyState.Definition.ProgressionTier + "\n" +
                "Unlocked: " + waterBodyState.IsUnlocked + "\n" +
                "Visited: " + waterBodyState.HasBeenVisited + "\n" +
                "Generated Challenges: " + waterBodyState.ActiveChallenges.Count + "\n" +
                "Completed Challenges: " + waterBodyState.CompletedChallengeCount
            );

            for (int j = 0; j < waterBodyState.ActiveChallenges.Count; j++) {
                ChallengeInstance challengeInstance = waterBodyState.ActiveChallenges[j];

                if (challengeInstance == null || challengeInstance.Definition == null) {
                    Debug.LogWarning("Challenge [" + j + "] in " + waterBodyState.Definition.DisplayName + " is null.");
                    continue;
                }

                Debug.Log(
                    "   Challenge [" + j + "]\n" +
                    "   Id: " + challengeInstance.Definition.Id + "\n" +
                    "   Name: " + challengeInstance.Definition.DisplayName + "\n" +
                    "   Objective Type: " + challengeInstance.Definition.ObjectiveType + "\n" +
                    "   Target Count: " + challengeInstance.Definition.TargetCount + "\n" +
                    "   Current Progress: " + challengeInstance.CurrentProgressValue + "\n" +
                    "   Completed: " + challengeInstance.IsCompleted + "\n" +
                    "   Claimed: " + challengeInstance.IsClaimed
                );
            }
        }

        Debug.Log("========================================");
    }

    [ContextMenu("Complete First Challenge In Each Water Body")]
    public void CompleteFirstChallengeInEachWaterBody() {
        if (gameSessionController == null || gameSessionController.CurrentRun == null) {
            Debug.LogWarning("RunBootstrapTester: No active run exists.");
            return;
        }

        RunState currentRun = gameSessionController.CurrentRun;

        for (int i = 0; i < currentRun.WaterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = currentRun.WaterBodies[i];

            if (waterBodyState == null || waterBodyState.Definition == null) {
                continue;
            }

            if (waterBodyState.ActiveChallenges == null || waterBodyState.ActiveChallenges.Count == 0) {
                continue;
            }

            ChallengeInstance firstChallenge = waterBodyState.ActiveChallenges[0];

            if (firstChallenge == null || firstChallenge.Definition == null) {
                continue;
            }

            gameSessionController.SetChallengeProgress(
                waterBodyState.Definition.Id,
                firstChallenge.Definition.Id,
                firstChallenge.Definition.TargetCount
            );
        }

        Debug.Log("RunBootstrapTester: Completed the first challenge in each water body.");
    }

    [ContextMenu("Unlock All Water Bodies")]
    public void UnlockAllWaterBodies() {
        if (gameSessionController == null || gameSessionController.CurrentRun == null) {
            Debug.LogWarning("RunBootstrapTester: No active run exists.");
            return;
        }

        RunState currentRun = gameSessionController.CurrentRun;

        for (int i = 0; i < currentRun.WaterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = currentRun.WaterBodies[i];

            if (waterBodyState == null) {
                continue;
            }

            waterBodyState.SetUnlocked(true);
        }

        Debug.Log("RunBootstrapTester: All water bodies unlocked.");
    }

    [ContextMenu("Simulate Catch")]
    public void SimulateCatch() {
        if (gameSessionController == null || gameSessionController.CurrentRun == null) {
            Debug.LogWarning("RunBootstrapTester: No active run.");
            return;
        }

        if (fishingInteractor == null) {
            Debug.LogWarning("RunBootstrapTester: FishingInteractor reference is missing.");
            return;
        }

        if (playerTackleBox == null) {
            Debug.LogWarning("RunBootstrapTester: TackleBox reference is missing.");
            return;
        }

        if (fishingInteractor.CurrentHotspot == null) {
            Debug.LogWarning("RunBootstrapTester: No current hotspot is available on FishingInteractor.");
            return;
        }

        FishSpawnTable spawnTable = fishingInteractor.CurrentHotspot.SpawnTable;

        if (spawnTable == null) {
            Debug.LogWarning("RunBootstrapTester: Current hotspot has no spawn table.");
            return;
        }

        FishRoll roll = spawnTable.Roll();

        if (roll.config == null) {
            Debug.LogWarning("RunBootstrapTester: Spawn table roll returned no fish species.");
            return;
        }

        FishSpeciesConfig species = roll.config;
        float fishSize01 = Mathf.Clamp01(roll.size01);
        bool isTrophy = roll.isTrophy;

        float length = 0f;
        float weight = 0f;

        bool resolved = species.TryResolveLengthWeight(fishSize01, out length, out weight);

        if (!resolved) {
            Debug.LogWarning("RunBootstrapTester: Failed to resolve fish length/weight.");
            return;
        }

        RodItem equippedRod = playerTackleBox.GetEquippedRod();
        ReelItem equippedReel = playerTackleBox.GetEquippedReel();
        LureItem equippedLure = playerTackleBox.GetEquippedLure();

        if (equippedRod == null) {
            Debug.LogWarning("RunBootstrapTester: No equipped rod found.");
            return;
        }

        if (equippedReel == null) {
            Debug.LogWarning("RunBootstrapTester: No equipped reel found.");
            return;
        }

        if (equippedLure == null) {
            Debug.LogWarning("RunBootstrapTester: No equipped lure found.");
            return;
        }

        CatchInfo catchInfo = new CatchInfo {
            species = species,
            fishSize01 = fishSize01,
            isTrophy = isTrophy,
            length = length,
            weight = weight
        };

        progressProcessor.ProcessFishCaught(catchInfo, equippedRod, equippedReel, equippedLure);

        Debug.Log(
            "RunBootstrapTester: Simulated catch processed.\n" +
            "Species: " + species.displayName + "\n" +
            "Size01: " + fishSize01 + "\n" +
            "Trophy: " + isTrophy + "\n" +
            "Length: " + length + "\n" +
            "Weight: " + weight + "\n" +
            "Rod: " + equippedRod.name + "\n" +
            "Reel: " + equippedReel.name + "\n" +
            "Lure: " + equippedLure.name
        );

        LogCurrentRunState();
    }

    [ContextMenu("Validate Generated Challenges")]
    public void ValidateGeneratedChallenges() {
        if (gameSessionController == null || gameSessionController.CurrentRun == null) {
            Debug.LogWarning("RunBootstrapTester: No active run exists.");
            return;
        }

        RunState currentRun = gameSessionController.CurrentRun;
        bool foundIssue = false;

        for (int i = 0; i < currentRun.WaterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = currentRun.WaterBodies[i];

            if (waterBodyState == null || waterBodyState.Definition == null) {
                continue;
            }

            bool hasDuplicateType = false;

            for (int j = 0; j < waterBodyState.ActiveChallenges.Count; j++) {
                ChallengeInstance a = waterBodyState.ActiveChallenges[j];

                if (a == null || a.Definition == null) {
                    continue;
                }

                for (int k = j + 1; k < waterBodyState.ActiveChallenges.Count; k++) {
                    ChallengeInstance b = waterBodyState.ActiveChallenges[k];

                    if (b == null || b.Definition == null) {
                        continue;
                    }

                    if (a.Definition.ObjectiveType == b.Definition.ObjectiveType) {
                        hasDuplicateType = true;
                        foundIssue = true;

                        Debug.LogError(
                            "RunBootstrapTester: Duplicate challenge type found in water body '" +
                            waterBodyState.Definition.DisplayName +
                            "'. Type: " + a.Definition.ObjectiveType +
                            " | Challenge A: " + a.Definition.DisplayName +
                            " | Challenge B: " + b.Definition.DisplayName
                        );
                    }
                }
            }

            if (!hasDuplicateType) {
                Debug.Log(
                    "RunBootstrapTester: Generation valid for water body '" +
                    waterBodyState.Definition.DisplayName +
                    "'. No duplicate challenge types found."
                );
            }
        }

        if (!foundIssue) {
            Debug.Log("RunBootstrapTester: All generated water bodies passed validation.");
        }
    }

    [ContextMenu("Simulate Matching Catch (Current Water Body)")]
    public void SimulateMatchingCatchForCurrentWaterBody() {
        if (gameSessionController == null || gameSessionController.CurrentRun == null) {
            Debug.LogWarning("RunBootstrapTester: No active run.");
            return;
        }

        if (playerTackleBox == null) {
            Debug.LogWarning("RunBootstrapTester: No TackleBox reference.");
            return;
        }

        FishSpawnTable spawnTable = fishingInteractor?.CurrentHotspot?.SpawnTable;

        if (spawnTable == null) {
            Debug.LogWarning("RunBootstrapTester: No spawn table available for matching simulation.");
            return;
        }

        WaterBodyRuntimeState waterBody = gameSessionController.GetCurrentWaterBody();

        if (waterBody == null || waterBody.ActiveChallenges == null || waterBody.ActiveChallenges.Count == 0) {
            Debug.LogWarning("RunBootstrapTester: No active challenges.");
            return;
        }

        ChallengeInstance targetChallenge = waterBody.ActiveChallenges[0];

        if (targetChallenge == null || targetChallenge.Definition == null) {
            Debug.LogWarning("RunBootstrapTester: First challenge is invalid.");
            return;
        }

        ChallengeDefinition definition = targetChallenge.Definition;

        FishSpeciesConfig species = null;
        RodItem rod = playerTackleBox.GetEquippedRod();
        ReelItem reel = playerTackleBox.GetEquippedReel();
        LureItem lure = playerTackleBox.GetEquippedLure();

        switch (definition.ObjectiveType) {
            case ChallengeObjectiveType.CatchTotalFish:
                species = GetSpawnableMatchingSpecies(spawnTable, definition);
                break;

            case ChallengeObjectiveType.CatchSpecies:
                species = GetFirstRequiredSpecies(definition);
                break;

            case ChallengeObjectiveType.CatchUsingRod:
                rod = GetFirstRequiredRod(definition) ?? rod;
                species = GetSpawnableMatchingSpecies(spawnTable, definition);
                break;

            case ChallengeObjectiveType.CatchUsingReel:
                reel = GetFirstRequiredReel(definition) ?? reel;
                species = GetSpawnableMatchingSpecies(spawnTable, definition);
                break;

            case ChallengeObjectiveType.CatchUsingLure:
                lure = GetFirstRequiredLure(definition) ?? lure;
                species = GetSpawnableMatchingSpecies(spawnTable, definition);
                break;

            default:
                species = GetSpawnableMatchingSpecies(spawnTable, definition);
                break;
        }

        if (species == null) {
            Debug.LogWarning("RunBootstrapTester: Failed to resolve species for matching test.");
            return;
        }

        float size01 = 0.5f;
        float length = 0f;
        float weight = 0f;

        if (!species.TryResolveLengthWeight(size01, out length, out weight)) {
            Debug.LogWarning("RunBootstrapTester: Failed to resolve fish length/weight.");
            return;
        }

        CatchInfo catchInfo = new CatchInfo {
            species = species,
            fishSize01 = size01,
            isTrophy = definition.RequireTrophy,
            length = length,
            weight = weight
        };

        progressProcessor.ProcessFishCaught(catchInfo, rod, reel, lure);

        Debug.Log("RunBootstrapTester: Simulated MATCHING catch for: " + definition.DisplayName);
    }

    FishSpeciesConfig GetFirstRequiredSpecies(ChallengeDefinition definition) {
        if (definition == null || definition.RequiredSpecies == null || definition.RequiredSpecies.Count == 0) {
            return null;
        }

        return definition.RequiredSpecies[0];
    }

    RodItem GetFirstRequiredRod(ChallengeDefinition definition) {
        if (definition == null || definition.RequiredRods == null || definition.RequiredRods.Count == 0) {
            return null;
        }

        return definition.RequiredRods[0];
    }

    ReelItem GetFirstRequiredReel(ChallengeDefinition definition) {
        if (definition == null || definition.RequiredReels == null || definition.RequiredReels.Count == 0) {
            return null;
        }

        return definition.RequiredReels[0];
    }

    LureItem GetFirstRequiredLure(ChallengeDefinition definition) {
        if (definition == null || definition.RequiredLures == null || definition.RequiredLures.Count == 0) {
            return null;
        }

        return definition.RequiredLures[0];
    }

    FishSpeciesConfig GetSpawnableMatchingSpecies(FishSpawnTable spawnTable, ChallengeDefinition definition) {
        if (spawnTable == null) {
            return null;
        }

        if (definition != null && definition.RequiredSpecies != null && definition.RequiredSpecies.Count > 0) {
            return definition.RequiredSpecies[0];
        }

        FishRoll roll = spawnTable.Roll();

        if (roll.config == null) {
            return null;
        }

        return roll.config;
    }
    #endregion
}