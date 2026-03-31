[System.Serializable]
public class ChallengeInstance {
    #region Variables
    ChallengeDefinition definition;
    int currentProgressValue;
    bool isCompleted;
    bool isClaimed;

    public ChallengeDefinition Definition => definition;
    public int CurrentProgressValue => currentProgressValue;
    public bool IsCompleted => isCompleted;
    public bool IsClaimed => isClaimed;

    #endregion

    #region Utility Methods

    public ChallengeInstance(ChallengeDefinition definition) {
        this.definition = definition;
        currentProgressValue = 0;
        isCompleted = false;
        isClaimed = false;
    }

    public void AddProgress(int amount) {
        if (definition == null || isCompleted || amount <= 0) {
            return;
        }

        currentProgressValue += amount;

        if (currentProgressValue >= definition.TargetCount) {
            currentProgressValue = definition.TargetCount;
            isCompleted = true;
        }
    }

    public void SetProgress(int value) {
        if (definition == null) {
            return;
        }

        currentProgressValue = System.Math.Max(0, value);

        if (currentProgressValue >= definition.TargetCount) {
            currentProgressValue = definition.TargetCount;
            isCompleted = true;
        }
        else {
            isCompleted = false;
        }
    }

    public void MarkClaimed() {
        if (!isCompleted) {
            return;
        }

        isClaimed = true;
    }

    #endregion
}