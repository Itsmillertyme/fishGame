using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSessionController : MonoBehaviour {
    #region Variables

    [Header("Run Setup")]
    [SerializeField] private List<WaterBodyDefinition> availableWaterBodies = new List<WaterBodyDefinition>();

    public static GameSessionController Instance { get; private set; }
    public RunState CurrentRun { get; private set; }

    public event Action<ChallengeInstance> OnChallengeUpdated;

    #endregion

    #region Unity Methods

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    #endregion

    #region Utility Methods

    public void StartNewRun() {
        string runId = Guid.NewGuid().ToString();
        CurrentRun = new RunState(runId);

        List<WaterBodyDefinition> sortedWaterBodies = GetSortedWaterBodies();

        for (int i = 0; i < sortedWaterBodies.Count; i++) {
            WaterBodyDefinition waterBodyDefinition = sortedWaterBodies[i];

            if (waterBodyDefinition == null) {
                continue;
            }

            bool isUnlocked = waterBodyDefinition.ProgressionTier == ProgressionTier.Tier1;
            List<ChallengeInstance> generatedChallenges = GenerateChallengesForWaterBody(waterBodyDefinition);

            WaterBodyRuntimeState waterBodyState = new WaterBodyRuntimeState(
                waterBodyDefinition,
                isUnlocked,
                generatedChallenges
            );

            CurrentRun.AddWaterBodyState(waterBodyState);
        }
    }

    public void EndRun() {
        CurrentRun = null;
    }

    public void SetCurrentWaterBody(string waterBodyId) {
        if (CurrentRun == null) {
            return;
        }

        WaterBodyRuntimeState waterBodyState = CurrentRun.GetWaterBodyById(waterBodyId);

        if (waterBodyState == null) {
            return;
        }

        CurrentRun.SetCurrentWaterBody(waterBodyState);
        waterBodyState.MarkVisited();
    }

    public WaterBodyRuntimeState GetCurrentWaterBody() {
        if (CurrentRun == null) {
            return null;
        }

        return CurrentRun.CurrentWaterBody;
    }

    public void AddProgressToChallenge(string waterBodyId, string challengeId, int amount) {
        if (CurrentRun == null || string.IsNullOrWhiteSpace(waterBodyId) || string.IsNullOrWhiteSpace(challengeId) || amount <= 0) {
            return;
        }

        WaterBodyRuntimeState waterBodyState = CurrentRun.GetWaterBodyById(waterBodyId);

        if (waterBodyState == null) {
            return;
        }

        ChallengeInstance challengeInstance = waterBodyState.GetChallengeById(challengeId);

        if (challengeInstance == null) {
            return;
        }

        challengeInstance.AddProgress(amount);
        waterBodyState.RefreshCompletedChallengeCount();

        OnChallengeUpdated?.Invoke(challengeInstance);
    }

    public void SetChallengeProgress(string waterBodyId, string challengeId, int value) {
        if (CurrentRun == null || string.IsNullOrWhiteSpace(waterBodyId) || string.IsNullOrWhiteSpace(challengeId)) {
            return;
        }

        WaterBodyRuntimeState waterBodyState = CurrentRun.GetWaterBodyById(waterBodyId);

        if (waterBodyState == null) {
            return;
        }

        ChallengeInstance challengeInstance = waterBodyState.GetChallengeById(challengeId);

        if (challengeInstance == null) {
            return;
        }

        challengeInstance.SetProgress(value);
        waterBodyState.RefreshCompletedChallengeCount();

        OnChallengeUpdated?.Invoke(challengeInstance);
    }

    public void ClaimChallenge(string waterBodyId, string challengeId) {
        if (CurrentRun == null || string.IsNullOrWhiteSpace(waterBodyId) || string.IsNullOrWhiteSpace(challengeId)) {
            return;
        }

        WaterBodyRuntimeState waterBodyState = CurrentRun.GetWaterBodyById(waterBodyId);

        if (waterBodyState == null) {
            return;
        }

        ChallengeInstance challengeInstance = waterBodyState.GetChallengeById(challengeId);

        if (challengeInstance == null) {
            return;
        }

        challengeInstance.MarkClaimed();
        waterBodyState.RefreshCompletedChallengeCount();

        OnChallengeUpdated?.Invoke(challengeInstance);
    }

    private List<WaterBodyDefinition> GetSortedWaterBodies() {
        List<WaterBodyDefinition> sortedWaterBodies = new List<WaterBodyDefinition>(availableWaterBodies);

        sortedWaterBodies.Sort((a, b) => {
            if (a == null && b == null) {
                return 0;
            }

            if (a == null) {
                return 1;
            }

            if (b == null) {
                return -1;
            }

            return a.ProgressionTier.CompareTo(b.ProgressionTier);
        });

        return sortedWaterBodies;
    }

    private List<ChallengeInstance> GenerateChallengesForWaterBody(WaterBodyDefinition waterBodyDefinition) {
        List<ChallengeInstance> generatedChallenges = new List<ChallengeInstance>();

        if (waterBodyDefinition == null || waterBodyDefinition.ChallengePool == null || waterBodyDefinition.ChallengePool.Count == 0) {
            return generatedChallenges;
        }

        //Build candidate pool
        List<ChallengeDefinition> candidatePool = new List<ChallengeDefinition>();

        for (int i = 0; i < waterBodyDefinition.ChallengePool.Count; i++) {
            ChallengeDefinition challengeDefinition = waterBodyDefinition.ChallengePool[i];

            if (challengeDefinition != null) {
                candidatePool.Add(challengeDefinition);
            }
        }

        if (candidatePool.Count == 0) {
            return generatedChallenges;
        }

        //Filter
        int uniqueTypeCount = GetUniqueObjectiveTypeCount(candidatePool);
        int challengeCount = waterBodyDefinition.GetRandomChallengeCount();
        challengeCount = Mathf.Clamp(challengeCount, 0, uniqueTypeCount);

        for (int i = 0; i < challengeCount; i++) {
            ChallengeObjectiveType selectedType;
            if (!TryGetWeightedRandomObjectiveType(candidatePool, out selectedType)) {
                break;
            }

            ChallengeDefinition selectedDefinition = GetWeightedRandomChallengeByType(candidatePool, selectedType);

            if (selectedDefinition == null) {
                break;
            }

            generatedChallenges.Add(new ChallengeInstance(selectedDefinition));
            RemoveChallengesOfType(candidatePool, selectedType);
        }

        return generatedChallenges;
    }

    private ChallengeDefinition GetWeightedRandomChallenge(List<ChallengeDefinition> candidatePool) {
        if (candidatePool == null || candidatePool.Count == 0) {
            return null;
        }

        float totalWeight = 0f;

        for (int i = 0; i < candidatePool.Count; i++) {
            ChallengeDefinition challengeDefinition = candidatePool[i];

            if (challengeDefinition == null) {
                continue;
            }

            totalWeight += Mathf.Max(0f, challengeDefinition.SelectionWeight);
        }

        if (totalWeight <= 0f) {
            return candidatePool[UnityEngine.Random.Range(0, candidatePool.Count)];
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float runningWeight = 0f;

        for (int i = 0; i < candidatePool.Count; i++) {
            ChallengeDefinition challengeDefinition = candidatePool[i];

            if (challengeDefinition == null) {
                continue;
            }

            runningWeight += Mathf.Max(0f, challengeDefinition.SelectionWeight);

            if (roll <= runningWeight) {
                return challengeDefinition;
            }
        }

        return candidatePool[candidatePool.Count - 1];

    }

    private int GetUniqueObjectiveTypeCount(List<ChallengeDefinition> candidatePool) {
        List<ChallengeObjectiveType> uniqueTypes = new List<ChallengeObjectiveType>();

        for (int i = 0; i < candidatePool.Count; i++) {
            ChallengeDefinition challengeDefinition = candidatePool[i];

            if (challengeDefinition == null) {
                continue;
            }

            bool alreadyExists = false;

            for (int j = 0; j < uniqueTypes.Count; j++) {
                if (uniqueTypes[j] == challengeDefinition.ObjectiveType) {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists) {
                uniqueTypes.Add(challengeDefinition.ObjectiveType);
            }
        }

        return uniqueTypes.Count;
    }

    private bool TryGetWeightedRandomObjectiveType(List<ChallengeDefinition> candidatePool, out ChallengeObjectiveType selectedType) {
        selectedType = ChallengeObjectiveType.CatchTotalFish;

        List<ChallengeObjectiveType> availableTypes = new List<ChallengeObjectiveType>();
        List<float> typeWeights = new List<float>();

        for (int i = 0; i < candidatePool.Count; i++) {
            ChallengeDefinition challengeDefinition = candidatePool[i];

            if (challengeDefinition == null) {
                continue;
            }

            ChallengeObjectiveType objectiveType = challengeDefinition.ObjectiveType;
            float weight = Mathf.Max(0f, challengeDefinition.SelectionWeight);

            int existingIndex = -1;

            for (int j = 0; j < availableTypes.Count; j++) {
                if (availableTypes[j] == objectiveType) {
                    existingIndex = j;
                    break;
                }
            }

            if (existingIndex >= 0) {
                typeWeights[existingIndex] += weight;
            }
            else {
                availableTypes.Add(objectiveType);
                typeWeights.Add(weight);
            }
        }

        if (availableTypes.Count == 0) {
            return false;
        }

        float totalWeight = 0f;

        for (int i = 0; i < typeWeights.Count; i++) {
            totalWeight += typeWeights[i];
        }

        if (totalWeight <= 0f) {
            selectedType = availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)];
            return true;
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float runningWeight = 0f;

        for (int i = 0; i < availableTypes.Count; i++) {
            runningWeight += typeWeights[i];

            if (roll <= runningWeight) {
                selectedType = availableTypes[i];
                return true;
            }
        }

        selectedType = availableTypes[availableTypes.Count - 1];
        return true;
    }

    private ChallengeDefinition GetWeightedRandomChallengeByType(List<ChallengeDefinition> candidatePool, ChallengeObjectiveType objectiveType) {
        List<ChallengeDefinition> matchingChallenges = new List<ChallengeDefinition>();

        for (int i = 0; i < candidatePool.Count; i++) {
            ChallengeDefinition challengeDefinition = candidatePool[i];

            if (challengeDefinition == null) {
                continue;
            }

            if (challengeDefinition.ObjectiveType == objectiveType) {
                matchingChallenges.Add(challengeDefinition);
            }
        }

        return GetWeightedRandomChallenge(matchingChallenges);
    }

    private void RemoveChallengesOfType(List<ChallengeDefinition> candidatePool, ChallengeObjectiveType objectiveType) {
        for (int i = candidatePool.Count - 1; i >= 0; i--) {
            ChallengeDefinition challengeDefinition = candidatePool[i];

            if (challengeDefinition == null) {
                candidatePool.RemoveAt(i);
                continue;
            }

            if (challengeDefinition.ObjectiveType == objectiveType) {
                candidatePool.RemoveAt(i);
            }
        }
    }

    #endregion
}
