//////using UnityEngine;

//////public class MobileInputBridge : MonoBehaviour {

//////    #region Variables
//////    [Header("References")]
//////    [SerializeField] CharacterControllerInputs characterInputs;
//////    [SerializeField] FishingInteractor fishingInteractor;

//////    [Header("Joysticks")]
//////    [SerializeField] VirtualJoystick moveJoystick;
//////    [SerializeField] VirtualJoystick lookJoystick;

//////    [Header("Look Settings")]
//////    [SerializeField] float lookSensitivity = 1.0f;
//////    [SerializeField] bool invertY = false;

//////    [Header("State")]
//////    [SerializeField] bool mobileInputEnabled = true;

//////    public bool MobileInputEnabled {
//////        get => mobileInputEnabled;
//////        set {
//////            mobileInputEnabled = value;
//////            if (!mobileInputEnabled) {
//////                ClearInputs();
//////            }
//////        }
//////    }
//////    #endregion

//////    #region Unity Methods
//////    private void Awake() {
//////        if (characterInputs == null) {
//////            characterInputs = GetComponent<CharacterControllerInputs>();
//////        }

//////        if (fishingInteractor == null) {
//////            fishingInteractor = GetComponent<FishingInteractor>();
//////        }
//////    }

//////    private void Update() {
//////        if (!mobileInputEnabled || characterInputs == null)
//////            return;

//////        Vector2 move = Vector2.zero;
//////        Vector2 look = Vector2.zero;

//////        if (moveJoystick != null) {
//////            move = moveJoystick.Value;
//////        }

//////        if (lookJoystick != null) {
//////            look = lookJoystick.Value * lookSensitivity;

//////            if (invertY) {
//////                look.y *= -1f;
//////            }
//////        }

//////        characterInputs.MoveInput(move);
//////        characterInputs.LookInput(look);
//////    }
//////    #endregion

//////    #region Utility Methods
//////    public void OnJumpPressed() {
//////        if (!mobileInputEnabled || characterInputs == null)
//////            return;

//////        characterInputs.JumpInput(true);
//////    }

//////    public void OnJumpReleased() {
//////        if (characterInputs == null)
//////            return;

//////        characterInputs.JumpInput(false);
//////    }

//////    public void OnCastPressed() {
//////        if (!mobileInputEnabled) return;

//////        if (fishingInteractor == null) {
//////            Debug.LogWarning("FishingInteractor reference is missing.");
//////            return;
//////        }

//////        fishingInteractor.OnCastPressed();
//////    }

//////    public void ClearInputs() {
//////        if (characterInputs != null) {
//////            characterInputs.MoveInput(Vector2.zero);
//////            characterInputs.LookInput(Vector2.zero);
//////            characterInputs.JumpInput(false);
//////            characterInputs.SprintInput(false);
//////        }

//////        if (moveJoystick != null) {
//////            moveJoystick.ResetJoystick();
//////        }

//////        if (lookJoystick != null) {
//////            lookJoystick.ResetJoystick();
//////        }
//////    }
//////    #endregion
//////}

////using UnityEngine;
////using UnityEngine.SceneManagement;

////public class MobileInputBridge : MonoBehaviour {

////    #region Variables
////    [Header("References")]
////    [SerializeField] CharacterControllerInputs characterInputs;
////    [SerializeField] FishingInteractor fishingInteractor;

////    [Header("Joysticks")]
////    [SerializeField] VirtualJoystick moveJoystick;
////    [SerializeField] VirtualJoystick lookJoystick;

////    [Header("Look Settings")]
////    [SerializeField] float lookSensitivity = 1.0f;
////    [SerializeField] bool invertY = false;

////    [Header("State")]
////    [SerializeField] bool mobileInputEnabled = true;

////    public bool MobileInputEnabled {
////        get => mobileInputEnabled;
////        set {
////            mobileInputEnabled = value;
////            if (!mobileInputEnabled) {
////                ClearInputs();
////            }
////        }
////    }
////    #endregion

////    #region Unity Methods
////    private void Awake() {
////        if (characterInputs == null) {
////            characterInputs = GetComponent<CharacterControllerInputs>();
////        }

////        if (fishingInteractor == null) {
////            fishingInteractor = GetComponent<FishingInteractor>();
////        }
////    }

////    private void OnEnable() {
////        SceneManager.activeSceneChanged += OnActiveSceneChanged;
////        SceneManager.sceneLoaded += OnSceneLoaded;
////    }

////    private void OnDisable() {
////        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
////        SceneManager.sceneLoaded -= OnSceneLoaded;

////        ClearInputs();
////    }

////    private void OnDestroy() {
////        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
////        SceneManager.sceneLoaded -= OnSceneLoaded;
////    }

////    private void Update() {
////        if (!mobileInputEnabled || characterInputs == null)
////            return;

////        Vector2 move = Vector2.zero;
////        Vector2 look = Vector2.zero;

////        if (moveJoystick != null) {
////            move = moveJoystick.Value;
////        }

////        if (lookJoystick != null) {
////            look = lookJoystick.Value * lookSensitivity;

////            if (invertY) {
////                look.y *= -1f;
////            }
////        }

////        characterInputs.MoveInput(move);
////        characterInputs.LookInput(look);
////    }
////    #endregion

////    #region Scene Methods
////    private void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
////        ClearInputs();

////        moveJoystick = null;
////        lookJoystick = null;
////    }

////    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
////        ClearInputs();

////        VirtualJoystick[] joysticks = FindObjectsByType<VirtualJoystick>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
////        foreach (var joystick in joysticks) {
////            if (joystick.name.Contains("Move")) {
////                moveJoystick = joystick;
////            }
////            else if (joystick.name.Contains("Look")) {
////                lookJoystick = joystick;
////            }
////        }
////    }
////    #endregion

////    #region Utility Methods
////    public void OnJumpPressed() {
////        if (!mobileInputEnabled || characterInputs == null)
////            return;

////        characterInputs.JumpInput(true);
////    }

////    public void OnJumpReleased() {
////        if (characterInputs == null)
////            return;

////        characterInputs.JumpInput(false);
////    }

////    public void OnCastPressed() {
////        if (!mobileInputEnabled) return;

////        if (fishingInteractor == null) {
////            Debug.LogWarning("FishingInteractor reference is missing.");
////            return;
////        }

////        fishingInteractor.OnCastPressed();
////    }

////    public void SetJoysticks(VirtualJoystick move, VirtualJoystick look) {
////        moveJoystick = move;
////        lookJoystick = look;
////        ClearInputs();
////    }

////    public void ClearInputs() {
////        if (characterInputs != null) {
////            characterInputs.MoveInput(Vector2.zero);
////            characterInputs.LookInput(Vector2.zero);
////            characterInputs.JumpInput(false);
////            characterInputs.SprintInput(false);
////            characterInputs.CastInput(false);
////            characterInputs.ZoomInput(0f);
////        }

////        if (moveJoystick != null) {
////            moveJoystick.ResetJoystick();
////        }

////        if (lookJoystick != null) {
////            lookJoystick.ResetJoystick();
////        }
////    }
////    #endregion
////}

//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class MobileInputBridge : MonoBehaviour {

//    #region Variables
//    [Header("References")]
//    [SerializeField] CharacterControllerInputs characterInputs;
//    [SerializeField] FishingInteractor fishingInteractor;

//    [Header("Joysticks")]
//    [SerializeField] VirtualJoystick moveJoystick;
//    [SerializeField] VirtualJoystick lookJoystick;

//    [Header("Look Settings")]
//    [SerializeField] float lookSensitivity = 1.0f;
//    [SerializeField] bool invertY = false;

//    [Header("State")]
//    [SerializeField] bool mobileInputEnabled = true;

//    public bool MobileInputEnabled {
//        get => mobileInputEnabled;
//        set {
//            mobileInputEnabled = value;
//            if (!mobileInputEnabled) {
//                ClearInputs();
//            }
//        }
//    }
//    #endregion

//    #region Unity Methods
//    private void Awake() {
//        if (characterInputs == null) {
//            characterInputs = GetComponent<CharacterControllerInputs>();
//        }

//        if (fishingInteractor == null) {
//            fishingInteractor = GetComponent<FishingInteractor>();
//        }
//    }

//    private void OnEnable() {
//        SceneManager.activeSceneChanged += OnActiveSceneChanged;
//        SceneManager.sceneLoaded += OnSceneLoaded;
//    }

//    private void OnDisable() {
//        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//        ClearInputs();
//    }

//    private void OnDestroy() {
//        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }

//    private void Update() {
//        if (!mobileInputEnabled || characterInputs == null)
//            return;

//        Vector2 move = Vector2.zero;
//        Vector2 look = Vector2.zero;

//        if (moveJoystick != null) {
//            move = moveJoystick.Value;
//        }

//        if (lookJoystick != null) {
//            look = lookJoystick.Value * lookSensitivity;

//            if (invertY) {
//                look.y *= -1f;
//            }
//        }

//        characterInputs.MoveInput(move);
//        characterInputs.LookInput(look);
//    }
//    #endregion

//    #region Utility Methods
//    private void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
//        ClearInputs();
//        moveJoystick = null;
//        lookJoystick = null;
//    }

//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
//        ClearInputs();
//    }

//    public void OnJumpPressed() {
//        if (!mobileInputEnabled || characterInputs == null)
//            return;

//        characterInputs.JumpInput(true);
//    }

//    public void OnJumpReleased() {
//        if (characterInputs == null)
//            return;

//        characterInputs.JumpInput(false);
//    }

//    public void OnCastPressed() {
//        if (!mobileInputEnabled)
//            return;

//        if (fishingInteractor == null) {
//            Debug.LogWarning("FishingInteractor reference is missing.");
//            return;
//        }

//        fishingInteractor.OnCastPressed();
//    }

//    public void SetJoysticks(VirtualJoystick move, VirtualJoystick look) {
//        moveJoystick = move;
//        lookJoystick = look;
//        ClearInputs();
//    }

//    public void ClearInputs() {
//        if (characterInputs != null) {
//            characterInputs.MoveInput(Vector2.zero);
//            characterInputs.LookInput(Vector2.zero);
//            characterInputs.JumpInput(false);
//            characterInputs.SprintInput(false);
//            characterInputs.CastInput(false);
//            characterInputs.ZoomInput(0f);
//        }

//        if (moveJoystick != null) {
//            moveJoystick.ResetJoystick();
//        }

//        if (lookJoystick != null) {
//            lookJoystick.ResetJoystick();
//        }
//    }
//    #endregion
//}

using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void OnEnable() {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ClearInputs();
    }

    private void OnDestroy() {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
    private void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
        ClearInputs();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        ClearInputs();

        // Let destroyed old-scene references fall away cleanly.
        if (moveJoystick == null) {
            moveJoystick = null;
        }

        if (lookJoystick == null) {
            lookJoystick = null;
        }
    }

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
        if (!mobileInputEnabled)
            return;

        if (fishingInteractor == null) {
            Debug.LogWarning("FishingInteractor reference is missing.");
            return;
        }

        fishingInteractor.OnCastPressed();
    }

    public void SetJoysticks(VirtualJoystick move, VirtualJoystick look) {
        moveJoystick = move;
        lookJoystick = look;
        ClearInputs();

        Debug.Log($"[MobileInputBridge] SetJoysticks | move={(moveJoystick != null ? moveJoystick.name : "null")} | look={(lookJoystick != null ? lookJoystick.name : "null")}");
    }

    public void ClearInputs() {
        if (characterInputs != null) {
            characterInputs.MoveInput(Vector2.zero);
            characterInputs.LookInput(Vector2.zero);
            characterInputs.JumpInput(false);
            characterInputs.SprintInput(false);
            characterInputs.CastInput(false);
            characterInputs.ZoomInput(0f);
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