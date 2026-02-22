using UnityEngine;

[CreateAssetMenu(fileName = "SimulationFishSpeciesPreset", menuName = "Simulation/Sim Fish Species Preset")]
public class SimulationFishSpeciesPreset : ScriptableObject {

    #region Variables
    [Header("Locomotion")]
    public float cruiseSpeed = 2.0f;
    public float schoolSpeed = 2.5f;
    public float maxSpeed = 4.0f;
    public float acceleration = 6.0f;      // units/sec^2
    public float turnRateDeg = 180.0f;     // degrees/sec
    public float bankAmountDeg = 20.0f;

    [Header("Schooling")]
    public float neighborRadius = 6.0f;
    public float separationDistance = 1.5f;
    public int enterSchoolNeighborCount = 4;
    public int exitSchoolNeighborCount = 2;

    [Header("Wander")]
    public float wanderStrength = 1.0f;
    public float wanderJitterRate = 1.5f; // how quickly wander direction changes

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 2.5f;
    public float avoidStrength = 2.0f;
    public float avoidProbeRadius = 0.25f;

    [Header("Habitat Bounds")]
    public float boundaryMargin = 2.0f;   // start correcting when within this distance to boundary
    public float boundaryStrength = 2.0f;

    [Header("Steering Weights - Cruise")]
    public float cruiseWanderWeight = 1.0f;
    public float cruiseCohesionWeight = 0.6f;
    public float cruiseAlignmentWeight = 0.4f;
    public float cruiseSeparationWeight = 1.0f;

    [Header("Steering Weights - School")]
    public float schoolWanderWeight = 0.3f;
    public float schoolCohesionWeight = 1.0f;
    public float schoolAlignmentWeight = 1.2f;
    public float schoolSeparationWeight = 1.0f;
    #endregion
}





