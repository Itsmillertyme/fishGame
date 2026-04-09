using System.Collections.Generic;
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
    [SerializeField] List<FishSpeciesConfig> requiredSpecies = new List<FishSpeciesConfig>();
    [SerializeField] List<RodItem> requiredRods = new List<RodItem>();
    [SerializeField] List<ReelItem> requiredReels = new List<ReelItem>();
    [SerializeField] List<LureItem> requiredLures = new List<LureItem>();
    [SerializeField] bool requireTrophy;

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public float SelectionWeight => selectionWeight;
    public ChallengeObjectiveType ObjectiveType => objectiveType;
    public int TargetCount => targetCount;
    public IReadOnlyList<FishSpeciesConfig> RequiredSpecies => requiredSpecies;
    public IReadOnlyList<RodItem> RequiredRods => requiredRods;
    public IReadOnlyList<ReelItem> RequiredReels => requiredReels;
    public IReadOnlyList<LureItem> RequiredLures => requiredLures;
    public bool RequireTrophy => requireTrophy;

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

        RemoveNulls(requiredSpecies);
        RemoveNulls(requiredRods);
        RemoveNulls(requiredReels);
        RemoveNulls(requiredLures);
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

    private void RemoveNulls<T>(List<T> list) where T : Object {
        if (list == null) {
            return;
        }

        for (int i = list.Count - 1; i >= 0; i--) {
            if (list[i] == null) {
                list.RemoveAt(i);
            }
        }
    }

    #endregion
}