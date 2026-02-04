using UnityEngine;
using UnityEngine.InputSystem;

public class MinigameInputRouter : MonoBehaviour {
    #region Variables
    [Header("Input Action References (Action Map: Minigame)")]
    [SerializeField] InputActionReference point;
    [SerializeField] InputActionReference dragDelta;
    [SerializeField] InputActionReference primary;
    [SerializeField] InputActionReference cancel;

    [Header("Options")]
    [SerializeField] bool autoEnableOnStart = false;

    [Header("Screen Regions")]
    [Tooltip("0.5 means left half / right half. Use 0.4 if you want a wider right-side 'reel' area.")]
    [Range(0.1f, 0.9f)]
    [SerializeField] float leftRegionMaxX01 = 0.5f;

    bool enabled;
    MinigameInput frameInput;
    #endregion

    #region Unity Methods
    void Start() {
        EnableMinigameInput(autoEnableOnStart);
    }

    void OnDisable() {
        EnableMinigameInput(false);
    }
    #endregion

    #region Utility Methods
    public void EnableMinigameInput(bool shouldEnable) {
        if (enabled == shouldEnable) return;
        enabled = shouldEnable;

        if (shouldEnable) {
            if (point != null) point.action.Enable();
            if (dragDelta != null) dragDelta.action.Enable();
            if (primary != null) primary.action.Enable();
            if (cancel != null) cancel.action.Enable();
        }
        else {
            if (point != null) point.action.Disable();
            if (dragDelta != null) dragDelta.action.Disable();
            if (primary != null) primary.action.Disable();
            if (cancel != null) cancel.action.Disable();
        }
    }

    public MinigameInput ConsumeFrameInput() {
        frameInput = new MinigameInput();

        if (!enabled) return frameInput;

        if (point != null) frameInput.pointerScreenPos = point.action.ReadValue<Vector2>();
        if (dragDelta != null) frameInput.dragDelta = dragDelta.action.ReadValue<Vector2>();

        if (primary != null) {
            frameInput.primaryDown = primary.action.WasPressedThisFrame();
            frameInput.primaryHeld = primary.action.IsPressed();
            frameInput.primaryUp = primary.action.WasReleasedThisFrame();
        }

        if (cancel != null) {
            frameInput.cancelDown = cancel.action.WasPressedThisFrame();
        }

        ApplyRegions(ref frameInput);
        return frameInput;
    }

    void ApplyRegions(ref MinigameInput input) {
        float screenWidth = Screen.width;
        float x01 = screenWidth > 0f ? Mathf.Clamp01(input.pointerScreenPos.x / screenWidth) : 0.5f;

        input.pointerX01 = x01;
        input.isLeftSide = x01 <= leftRegionMaxX01;
        input.isRightSide = !input.isLeftSide;
    }
    #endregion
}
