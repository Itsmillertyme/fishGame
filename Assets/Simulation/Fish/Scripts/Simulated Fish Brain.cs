using UnityEngine;
public class SimulatedFishBrain : MonoBehaviour {

    #region Variables
    [SerializeField] private SimFishState currentState = SimFishState.Cruise;
    #endregion

    #region Utility Methods
    public SimFishBrainOutput Tick(SimulationFishSpeciesPreset preset, SimFishPerceptionSnapshot snapshot) {
        // State switching
        if (currentState == SimFishState.Cruise) {
            if (snapshot.neighborCount >= preset.enterSchoolNeighborCount)
                currentState = SimFishState.School;
        }
        else // School
        {
            if (snapshot.neighborCount <= preset.exitSchoolNeighborCount)
                currentState = SimFishState.Cruise;
        }

        SimFishBrainOutput simFishBrainOutput = new SimFishBrainOutput();
        simFishBrainOutput.state = currentState;

        if (currentState == SimFishState.Cruise) {
            simFishBrainOutput.wanderWeight = preset.cruiseWanderWeight;
            simFishBrainOutput.cohesionWeight = preset.cruiseCohesionWeight;
            simFishBrainOutput.alignmentWeight = preset.cruiseAlignmentWeight;
            simFishBrainOutput.separationWeight = preset.cruiseSeparationWeight;
            simFishBrainOutput.targetSpeed = preset.cruiseSpeed;
        }
        else {
            simFishBrainOutput.wanderWeight = preset.schoolWanderWeight;
            simFishBrainOutput.cohesionWeight = preset.schoolCohesionWeight;
            simFishBrainOutput.alignmentWeight = preset.schoolAlignmentWeight;
            simFishBrainOutput.separationWeight = preset.schoolSeparationWeight;
            simFishBrainOutput.targetSpeed = preset.schoolSpeed;
        }

        // Clamp to max
        simFishBrainOutput.targetSpeed = Mathf.Min(simFishBrainOutput.targetSpeed, preset.maxSpeed);

        return simFishBrainOutput;
    }
    #endregion
}

#region Structs and Enums
public enum SimFishState {
    Cruise,
    School
}

public struct SimFishBrainOutput {
    public SimFishState state;

    // Weights chosen for current state
    public float wanderWeight;
    public float cohesionWeight;
    public float alignmentWeight;
    public float separationWeight;

    public float targetSpeed;
}
#endregion




