using System.Collections.Generic;
using UnityEngine;


public class SimulatedFishPerception : MonoBehaviour {

    #region Utility Methods
    public SimFishPerceptionSnapshot BuildSnapshot(SimulatedFishAgent agent) {
        SimFishPerceptionSnapshot snapshot = new SimFishPerceptionSnapshot();

        if (agent.manager == null || agent.preset == null)
            return snapshot;

        SimulationFishSpeciesPreset preset = agent.preset;
        List<SimulatedFishAgent> allFish = agent.manager.GetFishList();

        Vector3 pos = agent.transform.position;

        // Neighbor calculations
        Vector3 sumPositions = Vector3.zero;
        Vector3 sumForward = Vector3.zero;
        Vector3 separation = Vector3.zero;

        int count = 0;

        float neighborR2 = preset.neighborRadius * preset.neighborRadius;
        float sepDist = Mathf.Max(0.01f, preset.separationDistance);
        float sepR2 = sepDist * sepDist;

        for (int i = 0; i < allFish.Count; i++) {
            SimulatedFishAgent other = allFish[i];
            if (other == null || other == agent) continue;

            if (other.preset != agent.preset) continue;

            Vector3 to = other.transform.position - pos;
            float d2 = to.sqrMagnitude;

            if (d2 <= neighborR2) {
                count++;
                sumPositions += other.transform.position;
                sumForward += other.transform.forward;

                if (d2 <= sepR2 && d2 > 0.000001f) {
                    // Push away stronger when closer
                    float d = Mathf.Sqrt(d2);
                    float strength = 1.0f - Mathf.Clamp01(d / sepDist);
                    separation -= (to / d) * strength;
                }
            }
        }

        snapshot.neighborCount = count;

        if (count > 0) {
            Vector3 center = sumPositions / count;
            Vector3 toCenter = center - pos;
            snapshot.cohesionCenterDir = toCenter.sqrMagnitude > 0.0001f ? toCenter.normalized : Vector3.zero;

            snapshot.alignmentDir = sumForward.sqrMagnitude > 0.0001f ? sumForward.normalized : Vector3.zero;
            snapshot.separationDir = separation.sqrMagnitude > 0.0001f ? separation.normalized : Vector3.zero;
        }

        // Habitat boundary correction
        if (agent.habitat != null) {
            float distToBoundary;
            Vector3 correctionDir;
            bool inside;

            agent.habitat.Query(pos, preset.boundaryMargin, out distToBoundary, out correctionDir, out inside);

            snapshot.distanceToBoundary = distToBoundary;
            snapshot.boundaryCorrectionDir = correctionDir;
            snapshot.isInsideHabitat = inside;

            if (inside) {
                // urgency ramps up as you approach boundary margin
                if (distToBoundary <= preset.boundaryMargin) {
                    float t = 1.0f - Mathf.Clamp01(distToBoundary / Mathf.Max(0.0001f, preset.boundaryMargin));
                    snapshot.boundaryUrgency = t;
                }
                else snapshot.boundaryUrgency = 0f;
            }
            else {
                // outside = urgent
                snapshot.boundaryUrgency = 1f;
            }
        }

        // Obstacle avoidance (simple probe forward)
        LayerMask mask = agent.manager.ObstacleMask;
        Vector3 forward = agent.transform.forward;

        RaycastHit hit;
        bool blocked = Physics.SphereCast(pos, preset.avoidProbeRadius, forward, out hit, preset.avoidDistance, mask, QueryTriggerInteraction.Ignore);
        if (blocked) {
            float t = 1.0f - Mathf.Clamp01(hit.distance / Mathf.Max(0.0001f, preset.avoidDistance));
            snapshot.obstacleUrgency = t;

            // Reflect away from surface normal (simple, stable)
            Vector3 reflect = Vector3.Reflect(forward, hit.normal);
            snapshot.obstacleAvoidDir = reflect.sqrMagnitude > 0.0001f ? reflect.normalized : hit.normal.normalized;
        }

        return snapshot;
    }
    #endregion

}
#region Structs
public struct SimFishPerceptionSnapshot {
    public int neighborCount;

    // For schooling
    public Vector3 cohesionCenterDir;   // direction toward neighbors' center
    public Vector3 alignmentDir;        // direction to align with
    public Vector3 separationDir;       // direction pushing away from close neighbors

    // Safety
    public Vector3 obstacleAvoidDir;    // direction away from obstacle
    public float obstacleUrgency;       // 0..1

    public Vector3 boundaryCorrectionDir; // direction back into habitat
    public float boundaryUrgency;         // 0..1
    public float distanceToBoundary;
    public bool isInsideHabitat;
}
#endregion

