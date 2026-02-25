using UnityEngine;
using static SimulatedFishBrain;

public class SimulatedFishSteering : MonoBehaviour {

    #region Variables
    private Vector3 wanderDir = Vector3.forward;
    #endregion    

    #region Utility Methods
    public SimFishSteeringOutput Compute(SimulationFishSpeciesPreset preset, SimFishPerceptionSnapshot snapshot, SimFishBrainOutput brain, Transform fishTransform) {
        SimFishSteeringOutput steeringOutput = new SimFishSteeringOutput();

        // Wander direction: slowly drifting unit vector
        Vector3 jitter = Random.insideUnitSphere * preset.wanderJitterRate;
        Vector3 candidate = (wanderDir + jitter);
        if (candidate.sqrMagnitude < 0.0001f) candidate = fishTransform.forward;
        wanderDir = candidate.normalized;

        steeringOutput.wanderDir = wanderDir;
        steeringOutput.cohesionDir = snapshot.cohesionCenterDir;
        steeringOutput.alignmentDir = snapshot.alignmentDir;
        steeringOutput.separationDir = snapshot.separationDir;

        steeringOutput.obstacleAvoidDir = snapshot.obstacleAvoidDir;
        steeringOutput.boundaryDir = snapshot.boundaryCorrectionDir;

        // Blend schooling/wander
        Vector3 sum = Vector3.zero;

        sum += steeringOutput.wanderDir * (brain.wanderWeight * preset.wanderStrength);
        sum += steeringOutput.cohesionDir * brain.cohesionWeight;
        sum += steeringOutput.alignmentDir * brain.alignmentWeight;
        sum += steeringOutput.separationDir * brain.separationWeight;

        // Safety forces always apply, scaled by urgency
        if (snapshot.obstacleUrgency > 0f) {
            sum += steeringOutput.obstacleAvoidDir * (preset.avoidStrength * snapshot.obstacleUrgency);
        }

        if (snapshot.boundaryUrgency > 0f && steeringOutput.boundaryDir != Vector3.zero) {
            sum += steeringOutput.boundaryDir * (preset.boundaryStrength * snapshot.boundaryUrgency);
        }

        // Fallback if sum is tiny
        if (sum.sqrMagnitude < 0.0001f)
            sum = fishTransform.forward;

        steeringOutput.desiredDir = sum.normalized;
        steeringOutput.desiredSpeed = brain.targetSpeed;

        return steeringOutput;
    }
    #endregion

}

#region Structs
public struct SimFishSteeringOutput {
    public Vector3 wanderDir;
    public Vector3 cohesionDir;
    public Vector3 alignmentDir;
    public Vector3 separationDir;

    public Vector3 obstacleAvoidDir;
    public Vector3 boundaryDir;

    public Vector3 desiredDir;
    public float desiredSpeed;
}
#endregion

