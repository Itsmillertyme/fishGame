using UnityEngine;

public class SimulatedFishDebug : MonoBehaviour {
    #region Variables
    public SimulatedFishAgent Agent { get; set; }

    [Header("Debug Toggles")]
    public bool drawWhenSelected = true;
    public bool drawNeighborRadii = true;
    public bool drawVectors = true;
    #endregion

    #region Utility Methods
    private void OnDrawGizmosSelected() {
        if (!drawWhenSelected) return;

        SimulatedFishAgent agent = Agent;
        if (agent == null) agent = GetComponent<SimulatedFishAgent>();
        if (agent == null || agent.preset == null) return;

        SimulationFishSpeciesPreset preset = agent.preset;

        // Neighbor radii
        if (drawNeighborRadii) {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, preset.neighborRadius);

            Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, preset.separationDistance);
        }

        if (drawVectors) {
            Vector3 pos = transform.position;
            SimFishPerceptionSnapshot s = agent.LastPerception;
            SimFishSteeringOutput st = agent.LastSteering;

            float scale = 2.0f;

            // Desired direction (final)
            Gizmos.color = Color.white;
            Gizmos.DrawLine(pos, pos + st.desiredDir * scale);

            // Wander
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(pos, pos + st.wanderDir * (scale * 0.7f));

            // Cohesion
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos, pos + st.cohesionDir * (scale * 0.7f));

            // Alignment
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pos, pos + st.alignmentDir * (scale * 0.7f));

            // Separation
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + st.separationDir * (scale * 0.7f));

            // Obstacle avoid
            if (s.obstacleUrgency > 0f) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pos, pos + st.obstacleAvoidDir * (scale * (0.5f + s.obstacleUrgency)));
            }

            // Boundary correction
            if (s.boundaryUrgency > 0f && st.boundaryDir != Vector3.zero) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pos, pos + st.boundaryDir * (scale * (0.5f + s.boundaryUrgency)));
            }
        }

#if UNITY_EDITOR
        // Optional state label in Scene view
        UnityEditor.Handles.color = Color.white;
        Vector3 labelPos = transform.position + Vector3.up * 0.8f;
        UnityEditor.Handles.Label(labelPos, agent.LastBrain.state.ToString());
#endif
    }
    #endregion
}
