//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class GameSessionController : MonoBehaviour {
//    #region Variables

//    [Header("Run Setup")]
//    [SerializeField] private List<WaterBodyDefinition> availableWaterBodies = new List<WaterBodyDefinition>();

//    public static GameSessionController Instance { get; private set; }
//    public RunState CurrentRun { get; private set; }

//    #endregion

//    #region Unity Methods

//    private void Awake() {
//        if (Instance != null && Instance != this) {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        DontDestroyOnLoad(gameObject);
//    }

//    #endregion

//    #region Utility Methods

//    public void StartNewRun() {
//        string runId = Guid.NewGuid().ToString();
//        CurrentRun = new RunState(runId);

//        List<WaterBodyDefinition> sortedWaterBodies = GetSortedWaterBodies();

//        for (int i = 0; i < sortedWaterBodies.Count; i++) {
//            WaterBodyDefinition waterBodyDefinition = sortedWaterBodies[i];

//            if (waterBodyDefinition == null) {
//                continue;
//            }

//            bool isUnlocked = waterBodyDefinition.ProgressionTier == ProgressionTier.Tier1;
//            List<ChallengeInstance> generatedChallenges = GenerateChallengesForWaterBody(waterBodyDefinition);

//            WaterBodyRuntimeState waterBodyState = new WaterBodyRuntimeState(
//                waterBodyDefinition,
//                isUnlocked,
//                generatedChallenges
//            );

//            CurrentRun.AddWaterBodyState(waterBodyState);
//        }
//    }

//    public void EndRun() {
//        CurrentRun = null;
//    }

//    public void SetCurrentWaterBody(string waterBodyId) {
//        if (CurrentRun == null) {
//            return;
//        }

//        WaterBodyRuntimeState waterBodyState = CurrentRun.GetWaterBodyById(waterBodyId);

//        if (waterBodyState == null) {
//            return;
//        }

//        CurrentRun.SetCurrentWaterBody(waterBodyState);
//        waterBodyState.MarkVisited();
//    }

//    public WaterBodyRuntimeState GetCurrentWaterBody() {
//        if (CurrentRun == null) {
//            return null;
//        }

//        return CurrentRun.CurrentWaterBody;
//    }

//    private List<WaterBodyDefinition> GetSortedWaterBodies() {
//        List<WaterBodyDefinition> sortedWaterBodies = new List<WaterBodyDefinition>(availableWaterBodies);

//        sortedWaterBodies.Sort((a, b) => {
//            if (a == null && b == null) {
//                return 0;
//            }

//            if (a == null) {
//                return 1;
//            }

//            if (b == null) {
//                return -1;
//            }

//            return a.ProgressionTier.CompareTo(b.ProgressionTier);
//        });

//        return sortedWaterBodies;
//    }

//    private List<ChallengeInstance> GenerateChallengesForWaterBody(WaterBodyDefinition waterBodyDefinition) {
//        List<ChallengeInstance> generatedChallenges = new List<ChallengeInstance>();

//        if (waterBodyDefinition == null || waterBodyDefinition.ChallengePool == null || waterBodyDefinition.ChallengePool.Count == 0) {
//            return generatedChallenges;
//        }

//        List<ChallengeDefinition> candidatePool = new List<ChallengeDefinition>();

//        for (int i = 0; i < waterBodyDefinition.ChallengePool.Count; i++) {
//            ChallengeDefinition challengeDefinition = waterBodyDefinition.ChallengePool[i];

//            if (challengeDefinition != null) {
//                candidatePool.Add(challengeDefinition);
//            }
//        }

//        if (candidatePool.Count == 0) {
//            return generatedChallenges;
//        }

//        int challengeCount = waterBodyDefinition.GetRandomChallengeCount();
//        challengeCount = Mathf.Clamp(challengeCount, 0, candidatePool.Count);

//        for (int i = 0; i < challengeCount; i++) {
//            ChallengeDefinition selectedDefinition = GetWeightedRandomChallenge(candidatePool);

//            if (selectedDefinition == null) {
//                break;
//            }

//            generatedChallenges.Add(new ChallengeInstance(selectedDefinition));
//            candidatePool.Remove(selectedDefinition);
//        }

//        return generatedChallenges;
//    }

//    private ChallengeDefinition GetWeightedRandomChallenge(List<ChallengeDefinition> candidatePool) {
//        if (candidatePool == null || candidatePool.Count == 0) {
//            return null;
//        }

//        float totalWeight = 0f;

//        for (int i = 0; i < candidatePool.Count; i++) {
//            ChallengeDefinition challengeDefinition = candidatePool[i];

//            if (challengeDefinition == null) {
//                continue;
//            }

//            totalWeight += Mathf.Max(0f, challengeDefinition.SelectionWeight);
//        }

//        if (totalWeight <= 0f) {
//            return candidatePool[UnityEngine.Random.Range(0, candidatePool.Count)];
//        }

//        float roll = UnityEngine.Random.Range(0f, totalWeight);
//        float runningWeight = 0f;

//        for (int i = 0; i < candidatePool.Count; i++) {
//            ChallengeDefinition challengeDefinition = candidatePool[i];

//            if (challengeDefinition == null) {
//                continue;
//            }

//            runningWeight += Mathf.Max(0f, challengeDefinition.SelectionWeight);

//            if (roll <= runningWeight) {
//                return challengeDefinition;
//            }
//        }

//        return candidatePool[candidatePool.Count - 1];
//    }

//    #endregion
//}

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

        int challengeCount = waterBodyDefinition.GetRandomChallengeCount();
        challengeCount = Mathf.Clamp(challengeCount, 0, candidatePool.Count);

        for (int i = 0; i < challengeCount; i++) {
            ChallengeDefinition selectedDefinition = GetWeightedRandomChallenge(candidatePool);

            if (selectedDefinition == null) {
                break;
            }

            generatedChallenges.Add(new ChallengeInstance(selectedDefinition));
            candidatePool.Remove(selectedDefinition);
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

    #endregion
}
