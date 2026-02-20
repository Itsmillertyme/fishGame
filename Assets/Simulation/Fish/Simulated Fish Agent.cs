using UnityEngine;

[RequireComponent(typeof(SimulatedFishPerception))]
[RequireComponent(typeof(SimulatedFishBrain))]
[RequireComponent(typeof(SimulatedFishSteering))]
[RequireComponent(typeof(SimulatedFishMotor))]
[RequireComponent(typeof(SimulatedFishDebug))]
public class SimulatedFishAgent : MonoBehaviour {

    #region Variables
    [Header("Config")]
    public SimulationFishSpeciesPreset preset;

    [Header("Optional Visual Root (for banking/animation)")]
    public Transform visualRoot;

    [Header("Runtime (auto)")]
    public SimulatedFishManager manager;
    public HabitatVolume habitat;

    // Modules
    private SimulatedFishPerception perception;
    private SimulatedFishBrain brain;
    private SimulatedFishSteering steering;
    private SimulatedFishMotor motor;
    private SimulatedFishDebug debug;

    // Debug data (populated each tick)
    public SimFishPerceptionSnapshot LastPerception { get; private set; }
    public SimFishSteeringOutput LastSteering { get; private set; }
    public SimFishBrainOutput LastBrain { get; private set; }
    #endregion

    #region Unity Methods
    private void Awake() {
        perception = GetComponent<SimulatedFishPerception>();
        brain = GetComponent<SimulatedFishBrain>();
        steering = GetComponent<SimulatedFishSteering>();
        motor = GetComponent<SimulatedFishMotor>();
        debug = GetComponent<SimulatedFishDebug>();
    }

    private void Start() {
        if (manager == null) manager = SimulatedFishManager.Instance;
        if (manager != null) manager.Register(this);

        if (habitat == null && manager != null) habitat = manager.Habitat;
    }

    private void OnDisable() {
        if (manager != null) manager.Unregister(this);
    }

    private void Update() {
        if (preset == null) return;
        if (manager == null) manager = SimulatedFishManager.Instance;
        if (habitat == null && manager != null) habitat = manager.Habitat;

        // 1) Sense
        LastPerception = perception.BuildSnapshot(this);

        // 2) Decide (state + weights)
        LastBrain = brain.Tick(preset, LastPerception);

        // 3) Steer (blend vectors)
        LastSteering = steering.Compute(preset, LastPerception, LastBrain, transform);

        // 4) Move (turn + accel constraints)
        motor.Tick(preset, LastSteering, transform);

        // 5) Present (bank/anim)
        if (visualRoot != null) {
            motor.ApplyBankVisual(preset, LastSteering, transform, visualRoot);
        }

        // Debug component reads from this FishAgent
        debug.Agent = this;
    }
    #endregion
}
