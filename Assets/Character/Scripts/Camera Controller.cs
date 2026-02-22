using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
    #region Variables
    [Header("References")]
    [Tooltip("The follow target set in the Cinemachine Camera that the camera will follow (rotated for look).")]
    public GameObject cinemachineCameraTarget;

    [Tooltip("Cinemachine camera (CinemachineCamera) to zoom. In CM 3.x, reference the base type.")]
    public CinemachineVirtualCameraBase virtualCamera;

    [Tooltip("Inputs component on the player.")]
    public CharacterControllerInputs inputs;

    [Header("Look Settings")]
    [Tooltip("How far in degrees can you move the camera up")]
    public float topClamp = 65.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float bottomClamp = -45.0f;

    [Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
    public float cameraAngleOverride = 0.0f;

    [Space(5)]
    [Tooltip("Mouse look sensitivity multiplier.")]
    public float mouseLookSensitivity = 10.0f;

    [Tooltip("Gamepad look sensitivity multiplier.")]
    public float gamepadLookSensitivity = 120.0f;

    [Space(5)]
    [Tooltip("For locking the camera position on all axis")]
    public bool lockCameraPosition = false;

    [Header("Zoom Settings")]
    [Tooltip("Minimum zoom distance.")]
    public float minZoom = 1.5f;

    [Tooltip("Maximum zoom distance.")]
    public float maxZoom = 6.0f;

    [Tooltip("Scroll sensitivity for zoom normalized value (bigger = more zoom per scroll).")]
    public float zoomInputSensitivity = 0.08f;

    [Tooltip("How quickly the camera eases toward the target zoom distance.")]
    public float zoomSmoothSpeed = 10.0f;

    [Header("Zoom Curve")]
    [Tooltip("Zoom response curve from near (0) to far (1). Use EaseInOut for smooth ends.")]
    public AnimationCurve zoomResponse = null;

    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    private PlayerInput playerInput;

    private float zoomNormalized = 0.5f;   // 0 = minZoom, 1 = maxZoom
    private float currentDistance;
    private float targetDistance;

    private const float threshold = 0.01f;
    #endregion

    #region Unity Methods
    private void Awake() {
        playerInput = GetComponent<PlayerInput>();

        // If you didn't set a curve in the inspector, make a good default.
        if (zoomResponse == null) {
            zoomResponse = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }

    private void Start() {
        if (inputs == null) {
            inputs = GetComponent<CharacterControllerInputs>();
        }

        if (cinemachineCameraTarget != null) {
            cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
        }

        InitializeZoomFromCamera();
    }

    private void LateUpdate() {
        CameraRotation();
        CameraZoom();
    }
    #endregion

    #region Utility Methods
    private bool IsCurrentDeviceMouse() {
        return playerInput != null && playerInput.currentControlScheme == "KeyboardMouse";
    }

    private void InitializeZoomFromCamera() {
        if (virtualCamera == null) return;

        CinemachineComponentBase body = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);

        if (body is CinemachineThirdPersonFollow thirdPersonFollow) {
            currentDistance = thirdPersonFollow.CameraDistance;
            zoomNormalized = Mathf.InverseLerp(minZoom, maxZoom, currentDistance);
            targetDistance = currentDistance;
            return;
        }

        if (body is CinemachinePositionComposer positionComposer) {
            currentDistance = positionComposer.CameraDistance;
            zoomNormalized = Mathf.InverseLerp(minZoom, maxZoom, currentDistance);
            targetDistance = currentDistance;
            return;
        }

        // Fallback
        currentDistance = Mathf.Lerp(minZoom, maxZoom, 0.5f);
        targetDistance = currentDistance;
        zoomNormalized = 0.5f;
    }

    private void CameraRotation() {
        if (inputs == null) return;
        if (cinemachineCameraTarget == null) return;

        if (inputs.look.sqrMagnitude >= threshold && !lockCameraPosition) {
            bool usingMouse = IsCurrentDeviceMouse();

            float sensitivity = usingMouse ? mouseLookSensitivity : gamepadLookSensitivity;
            float deltaTimeMultiplier = usingMouse ? 1.0f : Time.deltaTime;

            cinemachineTargetYaw += inputs.look.x * sensitivity * deltaTimeMultiplier;
            cinemachineTargetPitch += inputs.look.y * sensitivity * deltaTimeMultiplier;
        }

        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(
            cinemachineTargetPitch + cameraAngleOverride,
            cinemachineTargetYaw,
            0.0f
        );
    }

    private void CameraZoom() {
        if (inputs == null) return;
        if (virtualCamera == null) return;

        float scroll = inputs.zoom;

        // Update the normalized zoom intent
        if (Mathf.Abs(scroll) > 0.01f) {
            zoomNormalized -= scroll * zoomInputSensitivity;
            zoomNormalized = Mathf.Clamp01(zoomNormalized);

            // Clear zoom so it doesn't get reused
            inputs.zoom = 0f;
        }

        // Use curve to shape the response
        float curved = zoomResponse.Evaluate(zoomNormalized);
        targetDistance = Mathf.Lerp(minZoom, maxZoom, curved);

        // Smoothly move toward target
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSmoothSpeed);

        CinemachineComponentBase body = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);

        if (body is CinemachineThirdPersonFollow thirdPersonFollow) {
            thirdPersonFollow.CameraDistance = currentDistance;
            return;
        }

        if (body is CinemachinePositionComposer positionComposer) {
            positionComposer.CameraDistance = currentDistance;
            return;
        }

        // Fallback
        if (virtualCamera is CinemachineCamera cmCamera) {
            float fov = cmCamera.Lens.FieldOfView;
            // Map distance intent to FOV a bit (optional fallback only)
            float fovTarget = Mathf.Lerp(25f, 75f, zoomNormalized);
            cmCamera.Lens.FieldOfView = Mathf.Lerp(fov, fovTarget, Time.deltaTime * zoomSmoothSpeed);
        }
    }

    private static float ClampAngle(float angle, float min, float max) {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
    #endregion
}