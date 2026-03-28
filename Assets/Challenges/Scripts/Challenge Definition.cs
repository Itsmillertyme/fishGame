using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Challenges/Challenge Definition", fileName = "ChallengeDefinition")]
public class ChallengeDefinition : ScriptableObject {
    #region Variables

    [Header("Identity")]
    [SerializeField] string id;
    [SerializeField] string displayName;
    [SerializeField][TextArea] string description;

    [Header("Generation")]
    [SerializeField] float selectionWeight = 1f;

    [Header("Objective")]
    [SerializeField] ChallengeObjectiveType objectiveType;
    [SerializeField] int targetCount = 1;

    [Header("Optional Filters")]
    [SerializeField] FishSpeciesConfig requiredSpecies;
    [SerializeField] RodItem requiredRod;
    [SerializeField] ReelItem requiredReel;
    [SerializeField] LureItem requiredLure;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public float SelectionWeight => selectionWeight;
    public ChallengeObjectiveType ObjectiveType => objectiveType;
    public int TargetCount => targetCount;
    public FishSpeciesConfig RequiredSpecies => requiredSpecies;
    public RodItem RequiredRod => requiredRod;
    public ReelItem RequiredReel => requiredReel;
    public LureItem RequiredLure => requiredLure;

    #endregion

    #region Unity Methods

#if UNITY_EDITOR
    private void OnValidate() {
        if (selectionWeight < 0f) {
            selectionWeight = 0f;
        }

        if (targetCount < 1) {
            targetCount = 1;
        }
    }
#endif

    #endregion

    #region Utility Methods

    public bool UsesSpecies() {
        return objectiveType == ChallengeObjectiveType.CatchSpecies;
    }

    public bool UsesRod() {
        return objectiveType == ChallengeObjectiveType.CatchUsingRod;
    }

    public bool UsesReel() {
        return objectiveType == ChallengeObjectiveType.CatchUsingReel;
    }

    public bool UsesLure() {
        return objectiveType == ChallengeObjectiveType.CatchUsingLure;
    }

    #endregion
}