using UnityEngine;

public class SimulatedFishMotor : MonoBehaviour {

    #region Variables
    private float currentSpeed = 0f;
    private float bankVel = 0f;
    private float bankCurrent = 0f;
    #endregion

    #region Utility Methods
    public void Tick(SimulationFishSpeciesPreset preset, SimFishSteeringOutput steering, Transform fishTransform) {
        float deltaTime = Time.deltaTime;

        // Smooth speed
        float targetSpeed = Mathf.Clamp(steering.desiredSpeed, 0f, preset.maxSpeed);
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, preset.acceleration * deltaTime);

        // Smooth rotation toward desired direction (limited turn rate)
        Quaternion currentRot = fishTransform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(steering.desiredDir, Vector3.up);
        float maxTurn = preset.turnRateDeg * deltaTime;
        fishTransform.rotation = Quaternion.RotateTowards(currentRot, targetRot, maxTurn);

        // Move forward
        fishTransform.position += fishTransform.forward * (currentSpeed * deltaTime);
    }

    public void ApplyBankVisual(SimulationFishSpeciesPreset preset, SimFishSteeringOutput steering, Transform bodyTransform, Transform visualRoot) {
        // Simple banking: bank more when turning more
        // Estimate turn intensity using angle between forward and desired direction
        Vector3 fishForward = bodyTransform.forward;
        Vector3 fishDesiredDir = steering.desiredDir;

        float angle = Vector3.Angle(fishForward, fishDesiredDir); // 0..180
        float bankTarget = Mathf.Clamp(angle / 45f, 0f, 1f) * preset.bankAmountDeg;

        // Determine sign using cross product around up axis
        Vector3 cross = Vector3.Cross(fishForward, fishDesiredDir);
        float sign = Mathf.Sign(Vector3.Dot(cross, bodyTransform.up));
        bankTarget *= sign;

        // Smooth bank
        bankCurrent = Mathf.SmoothDamp(bankCurrent, bankTarget, ref bankVel, 0.15f);

        Quaternion bankRot = Quaternion.AngleAxis(bankCurrent, Vector3.forward);
        visualRoot.localRotation = bankRot;
    }
    #endregion

}
