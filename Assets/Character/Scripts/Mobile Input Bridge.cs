using UnityEngine;

public class MobileInputBridge : MonoBehaviour {

    #region Variables
    [Header("References")]
    [SerializeField] CharacterControllerInputs characterInputs;
    [SerializeField] FishingInteractor fishingInteractor;

    [Header("Joysticks")]
    [SerializeField] VirtualJoystick moveJoystick;
    [SerializeField] VirtualJoystick lookJoystick;

    [Header("Look Settings")]
    [SerializeField] float lookSensitivity = 1.0f;
    [SerializeField] bool invertY = false;

    [Header("State")]
    [SerializeField] bool mobileInputEnabled = true;

    public bool MobileInputEnabled {
        get => mobileInputEnabled;
        set {
            mobileInputEnabled = value;
            if (!mobileInputEnabled) {
                ClearInputs();
            }
        }
    }
    #endregion

    #region Unity Methods
    private void Awake() {
        if (characterInputs == null) {
            characterInputs = GetComponent<CharacterControllerInputs>();
        }

        if (fishingInteractor == null) {
            fishingInteractor = GetComponent<FishingInteractor>();
        }
    }

    private void Update() {
        if (!mobileInputEnabled || characterInputs == null)
            return;

        Vector2 move = Vector2.zero;
        Vector2 look = Vector2.zero;

        if (moveJoystick != null) {
            move = moveJoystick.Value;
        }

        if (lookJoystick != null) {
            look = lookJoystick.Value * lookSensitivity;

            if (invertY) {
                look.y *= -1f;
            }
        }

        characterInputs.MoveInput(move);
        characterInputs.LookInput(look);
    }
    #endregion

    #region Utility Methods
    public void OnJumpPressed() {
        if (!mobileInputEnabled || characterInputs == null)
            return;

        characterInputs.JumpInput(true);
    }

    public void OnJumpReleased() {
        if (characterInputs == null)
            return;

        characterInputs.JumpInput(false);
    }

    public void OnCastPressed() {
        if (!mobileInputEnabled) return;

        if (fishingInteractor == null) {
            Debug.LogWarning("FishingInteractor reference is missing.");
            return;
        }

        fishingInteractor.OnCastPressed();
    }

    public void ClearInputs() {
        if (characterInputs != null) {
            characterInputs.MoveInput(Vector2.zero);
            characterInputs.LookInput(Vector2.zero);
            characterInputs.JumpInput(false);
            characterInputs.SprintInput(false);
        }

        if (moveJoystick != null) {
            moveJoystick.ResetJoystick();
        }

        if (lookJoystick != null) {
            lookJoystick.ResetJoystick();
        }
    }
    #endregion
}