using System.Collections.Generic;

[System.Serializable]
public class RunState {
    #region Variables

    string runId;
    List<WaterBodyRuntimeState> waterBodies;
    WaterBodyRuntimeState currentWaterBody;

    public string RunId => runId;
    public IReadOnlyList<WaterBodyRuntimeState> WaterBodies => waterBodies;
    public WaterBodyRuntimeState CurrentWaterBody => currentWaterBody;

    #endregion

    #region Utility Methods

    public RunState(string runId) {
        this.runId = runId;
        waterBodies = new List<WaterBodyRuntimeState>();
        currentWaterBody = null;
    }

    public void AddWaterBodyState(WaterBodyRuntimeState waterBodyState) {
        if (waterBodyState == null) {
            return;
        }

        waterBodies.Add(waterBodyState);

        if (currentWaterBody == null) {
            currentWaterBody = waterBodyState;
        }
    }

    public void SetCurrentWaterBody(WaterBodyRuntimeState waterBodyState) {
        currentWaterBody = waterBodyState;
    }

    public WaterBodyRuntimeState GetWaterBodyById(string waterBodyId) {
        if (string.IsNullOrWhiteSpace(waterBodyId)) {
            return null;
        }

        for (int i = 0; i < waterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = waterBodies[i];

            if (waterBodyState == null || waterBodyState.Definition == null) {
                continue;
            }

            if (waterBodyState.Definition.Id == waterBodyId) {
                return waterBodyState;
            }
        }

        return null;
    }

    public List<WaterBodyRuntimeState> GetWaterBodiesByTier(ProgressionTier progressionTier) {
        List<WaterBodyRuntimeState> matchingBodies = new List<WaterBodyRuntimeState>();

        for (int i = 0; i < waterBodies.Count; i++) {
            WaterBodyRuntimeState waterBodyState = waterBodies[i];

            if (waterBodyState == null || waterBodyState.Definition == null) {
                continue;
            }

            if (waterBodyState.Definition.ProgressionTier == progressionTier) {
                matchingBodies.Add(waterBodyState);
            }
        }

        return matchingBodies;
    }

    #endregion
}