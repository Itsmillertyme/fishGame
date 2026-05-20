using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChallengeUIController : MonoBehaviour {

    #region Variables

    [Header("References")]
    [SerializeField] private GameSessionController sessionController;
    [SerializeField] private List<TierContainerBinding> tierContainers = new List<TierContainerBinding>();

    [Header("UI")]
    [SerializeField] private TMP_Text challengeTextPrefab;

    private Dictionary<ProgressionTier, Transform> tierContainerLookup = new Dictionary<ProgressionTier, Transform>();
    #endregion

    #region Unity Methods
    private void Awake() {
        if (sessionController == null) {
            sessionController = GameSessionController.Instance;
        }

        BuildTierContainerLookup();
    }


    private void OnEnable() {
        if (sessionController == null) {
            sessionController = GameSessionController.Instance;
        }

        if (sessionController != null) {
            sessionController.OnChallengeUpdated += HandleChallengeUpdated;
        }

        BuildChallengeUI();
    }

    private void OnDisable() {
        if (sessionController != null) {
            sessionController.OnChallengeUpdated -= HandleChallengeUpdated;
        }
    }
    #endregion

    #region Utility Methods
    public void BuildChallengeUI() {
        if (sessionController == null) {
            sessionController = GameSessionController.Instance;
        }

        if (sessionController == null || sessionController.CurrentRun == null) {
            Debug.LogWarning("ChallengeUIController: No active run.");
            return;
        }

        ClearAllTierContainers();

        IReadOnlyList<WaterBodyRuntimeState> waterBodies = sessionController.CurrentRun.WaterBodies;

        for (int i = 0; i < waterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = waterBodies[i];

            if (waterBodyState == null || waterBodyState.Definition == null) {
                continue;
            }

            if (!tierContainerLookup.TryGetValue(waterBodyState.Definition.ProgressionTier, out Transform parentContainer) || parentContainer == null) {
                continue;
            }

            IReadOnlyList<ChallengeInstance> activeChallenges = waterBodyState.ActiveChallenges;

            if (activeChallenges == null) {
                continue;
            }

            for (int j = 0; j < activeChallenges.Count; j++) {
                ChallengeInstance challengeInstance = activeChallenges[j];

                if (challengeInstance == null || challengeInstance.Definition == null) {
                    continue;
                }

                CreateChallengeText(challengeInstance, parentContainer);
            }
        }
    }

    public void RefreshAllChallengeUI() {
        BuildChallengeUI();
    }

    void BuildTierContainerLookup() {
        tierContainerLookup.Clear();

        for (int i = 0; i < tierContainers.Count; i++) {
            TierContainerBinding binding = tierContainers[i];

            if (binding == null || binding.container == null) {
                continue;
            }

            tierContainerLookup[binding.tier] = binding.container;
        }
    }

    void CreateChallengeText(ChallengeInstance challengeInstance, Transform parentContainer) {
        if (challengeTextPrefab == null) {
            Debug.LogWarning("ChallengeUIController: No challengeTextPrefab assigned.");
            return;
        }

        TMP_Text newText = Instantiate(challengeTextPrefab, parentContainer);
        newText.transform.SetParent(parentContainer, false);
        newText.text = "\t" + GetChallengeDisplayText(challengeInstance);
        newText.fontSize = 14;
    }

    string GetChallengeDisplayText(ChallengeInstance challengeInstance) {
        string status = "";

        if (challengeInstance.IsClaimed) {
            status = " - Claimed";
        }
        else if (challengeInstance.IsCompleted) {
            status = " - Complete";
        }

        return challengeInstance.Definition.DisplayName +
               " (" +
               challengeInstance.CurrentProgressValue +
               "/" +
               challengeInstance.Definition.TargetCount +
               ")" +
               status;
    }

    private void ClearAllTierContainers() {
        foreach (KeyValuePair<ProgressionTier, Transform> pair in tierContainerLookup) {
            Transform container = pair.Value;

            if (container == null) {
                continue;
            }

            for (int i = container.childCount - 1; i >= 0; i--) {
                Destroy(container.GetChild(i).gameObject);
            }
        }
    }

    void HandleChallengeUpdated(ChallengeInstance instance) {
        RefreshAllChallengeUI();
    }

    #endregion

    #region Subclass
    [System.Serializable]
    public class TierContainerBinding {
        public ProgressionTier tier;
        public Transform container;
    }
    #endregion





}