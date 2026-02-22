using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterControllerInputs : MonoBehaviour {
    #region Variables
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool cast;

    [Tooltip("Mouse scroll wheel input for camera zoom.")]
    public float zoom;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;
    #endregion

    #region Unity Methods
    private void OnApplicationFocus(bool hasFocus) {
        SetCursorState(cursorLocked);
    }

    public void OnMove(InputValue value) {
        MoveInput(value.Get<Vector2>());
    }

    public void OnLook(InputValue value) {
        if (cursorInputForLook) {
            LookInput(value.Get<Vector2>());
        }
    }

    public void OnJump(InputValue value) {
        JumpInput(value.isPressed);
    }

    public void OnSprint(InputValue value) {
        SprintInput(value.isPressed);
    }

    public void OnCast(InputValue value) {
        FishingInteractor interactor = GetComponent<FishingInteractor>();
        if (interactor != null) {
            interactor.OnCastPressed();
            return;
        }


        Debug.Log("Error with FishingInteractor.cs");
    }

    public void OnZoom(InputValue value) {
        Vector2 scroll = value.Get<Vector2>();
        ZoomInput(scroll.y);
    }
    #endregion

    #region Utility Methods
    public void MoveInput(Vector2 newMoveDirection) {
        move = newMoveDirection;
    }

    public void LookInput(Vector2 newLookDirection) {
        look = newLookDirection;
    }

    public void JumpInput(bool newJumpState) {
        jump = newJumpState;
    }

    public void SprintInput(bool newSprintState) {
        sprint = newSprintState;
    }

    public void CastInput(bool newCastState) {
        cast = newCastState;
    }

    public void ZoomInput(float newZoomValue) {
        zoom = newZoomValue;
    }

    private void SetCursorState(bool newState) {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
    #endregion
}