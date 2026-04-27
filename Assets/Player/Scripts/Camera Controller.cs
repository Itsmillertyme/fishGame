using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
    #region Variables
    [Header("References")]
    [Tooltip("The follow target set in the Cinemachine Camera that the camera will follow (rotated for look).")]
    public GameObject cinemachineCameraTarget;

    [Tooltip("Inputs component on the player.")]
    public CharacterControllerInputs inputs;


    [Header("Virtual Cams")]
    [Tooltip("Your normal gameplay vcam (the one you rotate/zoom).")]
    public CinemachineVirtualCameraBase gameplayCam;

    [Tooltip("Your cutscene vcam (static framing / facing player).")]
    public CinemachineVirtualCameraBase cutsceneCam;

    [Header("Look Settings")]
    public float topClamp = 65.0f;
    public float bottomClamp = -45.0f;
    public float cameraAngleOverride = 0.0f;

    [Space(5)]
    public float mouseLookSensitivity = 10.0f;
    public float gamepadLookSensitivity = 120.0f;

    [Space(5)]
    public bool lockCameraPosition = false;

    [Header("Zoom Settings")]
    public float minZoom = 1.5f;
    public float maxZoom = 6.0f;
    public float zoomInputSensitivity = 0.08f;
    public float zoomSmoothSpeed = 10.0f;

    [Header("Zoom Curve")]
    public AnimationCurve zoomResponse = null;

    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    private PlayerInput playerInput;

    private float zoomNormalized = 0.5f;   // 0 = minZoom, 1 = maxZoom
    private float currentDistance;
    private float targetDistance;

    private const float threshold = 0.01f;

    private bool inCutscene = false;
    #endregion

    #region Unity Methods
    private void Awake() {
        playerInput = GetComponent<PlayerInput>();

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
        if (inCutscene) return;

        CameraRotation();
        CameraZoom();
    }
    #endregion

    #region Utility Methods
    public void EnterCutsceneCam() {
        if (cutsceneCam == null || gameplayCam == null) return;

        inCutscene = true;

        if (inputs != null) {
            inputs.zoom = 0f;
            inputs.look = Vector2.zero;
        }

        cutsceneCam.Priority += 10;
    }

    public void ExitCutsceneCam() {
        if (cutsceneCam == null || gameplayCam == null) return;

        inCutscene = false;

        cutsceneCam.Priority -= 10;

        if (cinemachineCameraTarget != null) {
            cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
            cinemachineTargetPitch = cinemachineCameraTarget.transform.rotation.eulerAngles.x;
        }

        InitializeZoomFromCamera();
    }

    private bool IsCurrentDeviceMouse() {
        return playerInput != null && playerInput.currentControlScheme == "KeyboardMouse";
    }

    private void InitializeZoomFromCamera() {
        // Zoom should read/write the gameplay cam (not cutscene cam)
        if (gameplayCam == null) return;

        CinemachineComponentBase body = gameplayCam.GetCinemachineComponent(CinemachineCore.Stage.Body);

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
        if (gameplayCam == null) return;

        float scroll = inputs.zoom;

        if (Mathf.Abs(scroll) > 0.01f) {
            zoomNormalized -= scroll * zoomInputSensitivity;
            zoomNormalized = Mathf.Clamp01(zoomNormalized);
            inputs.zoom = 0f;
        }

        float curved = zoomResponse.Evaluate(zoomNormalized);
        targetDistance = Mathf.Lerp(minZoom, maxZoom, curved);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSmoothSpeed);

        CinemachineComponentBase body = gameplayCam.GetCinemachineComponent(CinemachineCore.Stage.Body);

        if (body is CinemachineThirdPersonFollow thirdPersonFollow) {
            thirdPersonFollow.CameraDistance = currentDistance;
            return;
        }

        if (body is CinemachinePositionComposer positionComposer) {
            positionComposer.CameraDistance = currentDistance;
            return;
        }

        if (gameplayCam is CinemachineCamera cmCamera) {
            float fov = cmCamera.Lens.FieldOfView;
            float fovTarget = Mathf.Lerp(25f, 75f, zoomNormalized);
            cmCamera.Lens.FieldOfView = Mathf.Lerp(fov, fovTarget, Time.deltaTime * zoomSmoothSpeed);
        }
    }

    private static float ClampAngle(float angle, float min, float max) {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    public void SnapBehindPlayer(Transform playerTransform) {
        if (playerTransform == null) return;
        if (cinemachineCameraTarget == null) return;

        cinemachineTargetYaw = playerTransform.eulerAngles.y;
        cinemachineTargetPitch = 0f;

        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(
            cinemachineTargetPitch + cameraAngleOverride,
            cinemachineTargetYaw,
            0.0f
        );

        if (inputs != null) {
            inputs.look = Vector2.zero;
            inputs.zoom = 0f;
        }

        if (gameplayCam != null) {
            gameplayCam.PreviousStateIsValid = false;
        }
    }

    #endregion
}

