using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterControllerInputs))]
public class FishingInteractor : MonoBehaviour {

    #region Variables
    [Header("References")]
    [SerializeField] FishingMinigameController minigameController;
    [SerializeField] CharacterControllerThirdPerson playerController;

    [Header("DEBUG ONLY - Replace with tacklebox references")]
    [SerializeField] GameObject rodObject;
    [SerializeField] GameObject reelObject;

    [Header("Input Settings")]
    [Tooltip("MUST MATCH ACTION MAP IN INPUT ACTION SETTINGS")]
    [SerializeField] string gameplayMapName = "Player";
    [Tooltip("MUST MATCH ACTION MAP IN INPUT ACTION SETTINGS")]
    [SerializeField] string minigameMapName = "Minigame";

    [Header("Minigame Start Defaults (temporary)")]
    [SerializeField] bool useSpinningRod = true;
    [SerializeField] MinigameDifficulty difficulty = MinigameDifficulty.Medium;
    [SerializeField] bool extendedFight = false;

    [Header("State (Read Only)")]
    [SerializeField] bool canFish;
    [SerializeField] FishingHotspot currentHotspot;

    PlayerInput playerInput;
    CharacterControllerInputs inputs;
    public bool CanFish => currentHotspot != null;
    public bool IsFishing => inputs != null && inputs.cast;
    public FishingHotspot CurrentHotspot => currentHotspot;
    #endregion

    #region Unity Methods
    void Awake() {
        inputs = GetComponent<CharacterControllerInputs>();
        playerInput = GetComponent<PlayerInput>();
        if (playerController == null) playerController = GetComponent<CharacterControllerThirdPerson>();

        if (rodObject != null) rodObject.SetActive(false);
        if (reelObject != null) reelObject.SetActive(false);
    }

    void OnEnable() {
        if (minigameController != null) {
            minigameController.OnMinigameEnded += HandleMinigameEnded;
        }
    }

    void OnDisable() {
        if (minigameController != null) {
            minigameController.OnMinigameEnded -= HandleMinigameEnded;
        }
    }

    void Update() {
        // Keep inspector bool in sync 
        canFish = CanFish;

        bool shouldLock = (minigameController != null && minigameController.IsRunning);

        if (playerController != null) {
            playerController.MovementLocked = shouldLock;
        }

        if (shouldLock) {
            inputs.move = Vector2.zero;
            inputs.jump = false;
            inputs.sprint = false;
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!other.TryGetComponent(out FishingHotspot hotspot)) return;
        currentHotspot = hotspot;

        //Show rod, reel and tackle
        if (rodObject != null) rodObject.SetActive(true);
        if (reelObject != null) reelObject.SetActive(true);

        //UI pop up here??
    }

    void OnTriggerExit(Collider other) {
        if (!other.TryGetComponent(out FishingHotspot hotspot)) return;
        if (currentHotspot == hotspot) currentHotspot = null;

        //Hide rod, reel and tackle
        if (rodObject != null) rodObject.SetActive(false);
        if (reelObject != null) reelObject.SetActive(false);

        //Need END FIGHT logic
    }
    #endregion

    #region Utility Methods
    public void OnCastPressed() {
        if (minigameController == null) {
            if (!CanFish) return;
            inputs.cast = !inputs.cast;
            return;
        }

        // If minigame is running, treat cast as "cancel/stop"
        if (minigameController.IsRunning) {
            StopFishing();
            return;
        }
        else {
            TryStartFishing();
        }
    }

    public void TryStartFishing() {
        if (!CanFish) return;

        inputs.cast = true;

        //Choose fish from spawn table here

        if (useSpinningRod) {
            minigameController.StartSpinningRodMinigame(difficulty, extendedFight);
        }
        else {
            minigameController.StartCastingRodMinigame(difficulty, extendedFight);
        }

        SwitchToMinigameMap();
    }

    public void StopFishing() {
        inputs.cast = false;

        if (minigameController != null) {
            minigameController.StopMinigame(MinigameEndReason.Cancelled);
        }

        SwitchToGameplayMap();
    }

    void HandleMinigameEnded(MinigameResult result) {

        inputs.cast = false;

        SwitchToGameplayMap();
    }

    void SwitchToMinigameMap() {
        if (playerInput != null) playerInput.SwitchCurrentActionMap(minigameMapName);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void SwitchToGameplayMap() {
        if (playerInput != null) playerInput.SwitchCurrentActionMap(gameplayMapName);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion

}