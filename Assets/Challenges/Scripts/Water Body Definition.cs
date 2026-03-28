using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Challenges/Water Body Definition", fileName = "WaterBodyDefinition")]
public class WaterBodyDefinition : ScriptableObject {
    #region Variables

    [Header("Identity")]
    [SerializeField] string id;
    [SerializeField] string displayName;
    [SerializeField] string sceneName;
    [SerializeField] FishSpawnTable spawnTable;

    [Header("Progression")]
    [SerializeField] ProgressionTier progressionTier;

    [Header("Generation")]
    [SerializeField] int minChallengesToGenerate = 3;
    [SerializeField] int maxChallengesToGenerate = 5;

    [Header("Challenge Pool")]
    [SerializeField] List<ChallengeDefinition> challengePool = new List<ChallengeDefinition>();

    public string Id => id;
    public string DisplayName => displayName;
    public string SceneName => sceneName;
    public ProgressionTier ProgressionTier => progressionTier;
    public int MinChallengesToGenerate => minChallengesToGenerate;
    public int MaxChallengesToGenerate => maxChallengesToGenerate;
    public IReadOnlyList<ChallengeDefinition> ChallengePool => challengePool;

    #endregion

    #region Unity Methods

#if UNITY_EDITOR
    private void OnValidate() {
        if (minChallengesToGenerate < 0) {
            minChallengesToGenerate = 0;
        }

        if (maxChallengesToGenerate < 0) {
            maxChallengesToGenerate = 0;
        }

        if (maxChallengesToGenerate < minChallengesToGenerate) {
            maxChallengesToGenerate = minChallengesToGenerate;
        }
    }
#endif

    #endregion

    #region Utility Methods

    public int GetRandomChallengeCount() {
        if (challengePool == null || challengePool.Count == 0) {
            return 0;
        }

        int clampedMin = Mathf.Clamp(minChallengesToGenerate, 0, challengePool.Count);
        int clampedMax = Mathf.Clamp(maxChallengesToGenerate, clampedMin, challengePool.Count);

        return Random.Range(clampedMin, clampedMax + 1);
    }

    #endregion
}