using UnityEngine;

public class RunBootstrapTester : MonoBehaviour {
    #region Variables

    [Header("References")]
    [SerializeField] GameSessionController gameSessionController;
    [SerializeField] TackleBox playerTackleBox;
    [SerializeField] FishingInteractor fishingInteractor;

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

        if (markCurrentBodyVisited) {
            WaterBodyRuntimeState currentWaterBody = gameSessionController.GetCurrentWaterBody();

            if (currentWaterBody != null) {
                currentWaterBody.MarkVisited();
            }
        }

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

            if (waterBodyState == null || waterBodyState.ActiveChallenges == null || waterBodyState.ActiveChallenges.Count == 0) {
                continue;
            }

            ChallengeInstance firstChallenge = waterBodyState.ActiveChallenges[0];

            if (firstChallenge == null || firstChallenge.Definition == null) {
                continue;
            }

            firstChallenge.SetProgress(firstChallenge.Definition.TargetCount);
            waterBodyState.RefreshCompletedChallengeCount();
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

    #endregion
}