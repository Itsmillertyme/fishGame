using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterControllerInputs))]
public class FishingInteractor : MonoBehaviour {

    #region Variables 
    [Header("References")]
    [SerializeField] FishingMinigameController minigameController;
    [SerializeField] CharacterControllerThirdPerson playerController;
    [SerializeField] TackleBox tackleBox;

    [Header("DEBUG ONLY - Replace with tacklebox references")]
    [SerializeField] GameObject rodObject;
    [SerializeField] GameObject reelObject;

    [Header("Input Settings")]
    [Tooltip("MUST MATCH ACTION MAP IN INPUT ACTION SETTINGS")]
    [SerializeField] string gameplayMapName = "Player";
    [Tooltip("MUST MATCH ACTION MAP IN INPUT ACTION SETTINGS")]
    [SerializeField] string minigameMapName = "Minigame";

    [Header("Minigame Start Defaults (temporary)")]
    [SerializeField] MinigameDifficulty difficulty = MinigameDifficulty.Medium;
    [SerializeField] bool extendedFight = false;

    [Header("Bite Delay (Base)")]
    [Tooltip("Base bite delay before lure modifiers are applied.")]
    [SerializeField] float baseBiteDelaySeconds = 0.25f;

    [Tooltip("Random +/- variance applied after lure modifiers. Set to 0 to disable.")]
    [SerializeField] float biteDelayVarianceSeconds = 0.15f;

    [Header("State (Read Only)")]
    [SerializeField] bool canFish;
    [SerializeField] FishingHotspot currentHotspot;

    PlayerInput playerInput;
    CharacterControllerInputs inputs;

    Coroutine biteRoutine;

    bool castLeadInComplete;
    bool hasPendingCatch;
    ReelType pendingReelType;
    FishSpeciesConfig pendingSpecies;
    float pendingSize01;
    TackleModifiers pendingTackle;

    public bool CanFish => currentHotspot != null;
    public bool IsFishing => inputs != null && inputs.cast;
    public FishingHotspot CurrentHotspot => currentHotspot;
    #endregion

    #region Unity Methods
    void Awake() {
        inputs = GetComponent<CharacterControllerInputs>();
        playerInput = GetComponent<PlayerInput>();
        if (playerController == null) playerController = GetComponent<CharacterControllerThirdPerson>();
        if (tackleBox == null) tackleBox = GetComponent<TackleBox>();

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

        bool waitingForBite = biteRoutine != null;

        if (playerController != null) {
            playerController.MovementLocked = shouldLock || waitingForBite;
        }

        if (shouldLock || waitingForBite) {
            inputs.move = Vector2.zero;
            inputs.jump = false;
            inputs.sprint = false;
        }
        bool waitingForCast = IsFishing && !castLeadInComplete && biteRoutine == null && (minigameController == null || !minigameController.IsRunning);
    }

    void OnTriggerEnter(Collider other) {
        if (!other.TryGetComponent(out FishingHotspot hotspot)) return;
        currentHotspot = hotspot;

        // Show rod, reel and tackle
        if (rodObject != null) rodObject.SetActive(true);
        if (reelObject != null) reelObject.SetActive(true);

        // UI pop up here??
    }

    void OnTriggerExit(Collider other) {
        if (!other.TryGetComponent(out FishingHotspot hotspot)) return;
        if (currentHotspot == hotspot) currentHotspot = null;

        // Hide rod, reel and tackle
        if (rodObject != null) rodObject.SetActive(false);
        if (reelObject != null) reelObject.SetActive(false);

        // If we walked out while waiting or fishing, stop.
        if (IsFishing || biteRoutine != null) {
            StopFishing();
        }
    }
    #endregion

    #region Utility Methods
    public void OnCastPressed() {
        if (!CanFish) return;

        if (minigameController == null) {
            inputs.cast = !inputs.cast;
            return;
        }

        // If minigame is running, treat cast as "cancel/stop"
        if (minigameController.IsRunning || biteRoutine != null) {
            StopFishing();
            return;
        }

        TryStartFishing();
    }

    public void TryStartFishing() {
        if (!CanFish) return;
        if (minigameController == null) return;

        inputs.cast = true;
        castLeadInComplete = false;

        // Choose fish
        FishSpawnTable spawnTable = currentHotspot.SpawnTable;
        FishRoll roll = spawnTable.Roll();
        FishSpeciesConfig species = roll.config;
        float fishSize01 = roll.size01;

        // Determine setup type from tackle box
        ReelType reelType = ReelType.Spinning;
        if (tackleBox != null) {
            reelType = tackleBox.GetEquippedSetupType();
        }

        // Resolve tackle + bite delay
        TackleModifiers tackle = TackleModifiers.Default();
        if (tackleBox != null) {
            tackle = tackleBox.GetCurrentTackleModifiers();
        }

        pendingSpecies = species;
        pendingSize01 = fishSize01;
        pendingTackle = tackle;
        pendingReelType = reelType;
        hasPendingCatch = true;
    }

    IEnumerator StartMinigameAfterDelay(ReelType reelType, FishSpeciesConfig species, float fishSize01, TackleModifiers tackle, float delaySeconds) {
        float timer = 0f;
        while (timer < delaySeconds) {
            // Cancel conditions
            if (!CanFish) {
                StopFishing();
                yield break;
            }
            if (!IsFishing) {
                yield break;
            }
            if (minigameController == null) {
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        biteRoutine = null;

        // Start minigame after delay
        StartMinigameNow(reelType, species, fishSize01, tackle);
    }

    void StartMinigameNow(ReelType reelType, FishSpeciesConfig species, float fishSize01, TackleModifiers tackle) {
        if (minigameController == null) return;

        if (reelType == ReelType.Spinning) {
            minigameController.StartSpinningRodMinigame(species, fishSize01, difficulty, extendedFight, tackle);
        }
        else {
            minigameController.StartCastingRodMinigame(species, fishSize01, difficulty, extendedFight, tackle);
        }
        SwitchToMinigameMap();
    }

    float ResolveBiteDelaySeconds(TackleModifiers tackle) {
        float delay = Mathf.Max(0f, baseBiteDelaySeconds);

        // Apply lure modifiers
        delay = (delay + tackle.biteDelayAddSeconds) * Mathf.Max(0.05f, tackle.biteDelayMultiplier);

        if (biteDelayVarianceSeconds > 0f) {
            float v = Random.Range(-biteDelayVarianceSeconds, biteDelayVarianceSeconds);
            delay += v;
        }

        return Mathf.Max(0f, delay);
    }

    public void StopFishing() {
        inputs.cast = false;
        castLeadInComplete = false;
        hasPendingCatch = false;

        if (biteRoutine != null) {
            StopCoroutine(biteRoutine);
            biteRoutine = null;
        }

        if (minigameController != null) {
            minigameController.StopMinigame(MinigameEndReason.Cancelled);
        }

        SwitchToGameplayMap();
    }

    void HandleMinigameEnded(MinigameResult result) {
        inputs.cast = false;
        castLeadInComplete = false;
        hasPendingCatch = false;

        if (biteRoutine != null) {
            StopCoroutine(biteRoutine);
            biteRoutine = null;
        }

        if (playerController != null) playerController.MovementLocked = false;

        SwitchToGameplayMap();
    }

    public void OnCastAnimationComplete() {
        castLeadInComplete = true;

        if (!IsFishing) return;
        if (!CanFish) { StopFishing(); return; }
        if (minigameController == null) return;

        if (!hasPendingCatch) return;

        // Start bite delay now
        float delay = ResolveBiteDelaySeconds(pendingTackle);

        // If no delay, start immediately
        if (delay <= 0f) {
            StartMinigameNow(pendingReelType, pendingSpecies, pendingSize01, pendingTackle);
            return;
        }

        if (biteRoutine != null) {
            StopCoroutine(biteRoutine);
            biteRoutine = null;
        }

        biteRoutine = StartCoroutine(StartMinigameAfterDelay(
            pendingReelType, pendingSpecies, pendingSize01, pendingTackle, delay
        ));
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

//using UnityEngine;
//using UnityEngine.InputSystem;

//[RequireComponent(typeof(CharacterControllerInputs))]
//public class FishingInteractor : MonoBehaviour {

//    #region Variables
//    [Header("References")]
//    [SerializeField] FishingMinigameController minigameController;
//    [SerializeField] CharacterControllerThirdPerson playerController;

//    [Header("DEBUG ONLY - Replace with tacklebox references")]
//    [SerializeField] GameObject rodObject;
//    [SerializeField] GameObject reelObject;

//    [Header("Input Settings")]
//    [Tooltip("MUST MATCH ACTION MAP IN INPUT ACTION SETTINGS")]
//    [SerializeField] string gameplayMapName = "Player";
//    [Tooltip("MUST MATCH ACTION MAP IN INPUT ACTION SETTINGS")]
//    [SerializeField] string minigameMapName = "Minigame";

//    [Header("Minigame Start Defaults (temporary)")]
//    [SerializeField] bool useSpinningRod = true;
//    [SerializeField] MinigameDifficulty difficulty = MinigameDifficulty.Medium;
//    [SerializeField] bool extendedFight = false;

//    [Header("State (Read Only)")]
//    [SerializeField] bool canFish;
//    [SerializeField] FishingHotspot currentHotspot;

//    PlayerInput playerInput;
//    CharacterControllerInputs inputs;
//    public bool CanFish => currentHotspot != null;
//    public bool IsFishing => inputs != null && inputs.cast;
//    public FishingHotspot CurrentHotspot => currentHotspot;
//    #endregion

//    #region Unity Methods
//    void Awake() {
//        inputs = GetComponent<CharacterControllerInputs>();
//        playerInput = GetComponent<PlayerInput>();
//        if (playerController == null) playerController = GetComponent<CharacterControllerThirdPerson>();

//        if (rodObject != null) rodObject.SetActive(false);
//        if (reelObject != null) reelObject.SetActive(false);
//    }

//    void OnEnable() {
//        if (minigameController != null) {
//            minigameController.OnMinigameEnded += HandleMinigameEnded;
//        }
//    }

//    void OnDisable() {
//        if (minigameController != null) {
//            minigameController.OnMinigameEnded -= HandleMinigameEnded;
//        }
//    }

//    void Update() {
//        // Keep inspector bool in sync 
//        canFish = CanFish;

//        bool shouldLock = (minigameController != null && minigameController.IsRunning);

//        if (playerController != null) {
//            playerController.MovementLocked = shouldLock;
//        }

//        if (shouldLock) {
//            inputs.move = Vector2.zero;
//            inputs.jump = false;
//            inputs.sprint = false;
//        }
//    }

//    void OnTriggerEnter(Collider other) {
//        if (!other.TryGetComponent(out FishingHotspot hotspot)) return;
//        currentHotspot = hotspot;

//        //Show rod, reel and tackle
//        if (rodObject != null) rodObject.SetActive(true);
//        if (reelObject != null) reelObject.SetActive(true);

//        //UI pop up here??
//    }

//    void OnTriggerExit(Collider other) {
//        if (!other.TryGetComponent(out FishingHotspot hotspot)) return;
//        if (currentHotspot == hotspot) currentHotspot = null;

//        //Hide rod, reel and tackle
//        if (rodObject != null) rodObject.SetActive(false);
//        if (reelObject != null) reelObject.SetActive(false);

//        //Need END FIGHT logic
//    }
//    #endregion

//    #region Utility Methods
//    public void OnCastPressed() {
//        if (minigameController == null) {
//            if (!CanFish) return;
//            inputs.cast = !inputs.cast;
//            return;
//        }

//        // If minigame is running, treat cast as "cancel/stop"
//        if (minigameController.IsRunning) {
//            StopFishing();
//            return;
//        }
//        else {
//            TryStartFishing();
//        }
//    }

//    public void TryStartFishing() {
//        if (!CanFish) return;

//        inputs.cast = true;
//        playerController.MovementLocked = true;

//        //Choose fish from spawn table here

//        if (useSpinningRod) {
//            minigameController.StartSpinningRodMinigame(difficulty, extendedFight);
//        }
//        else {
//            minigameController.StartCastingRodMinigame(difficulty, extendedFight);
//        }

//        SwitchToMinigameMap();
//    }

//    public void StopFishing() {
//        inputs.cast = false;

//        if (minigameController != null) {
//            minigameController.StopMinigame(MinigameEndReason.Cancelled);
//        }

//        SwitchToGameplayMap();
//    }

//    void HandleMinigameEnded(MinigameResult result) {

//        inputs.cast = false;
//        playerController.MovementLocked = false;

//        SwitchToGameplayMap();
//    }

//    void SwitchToMinigameMap() {
//        if (playerInput != null) playerInput.SwitchCurrentActionMap(minigameMapName);

//        Cursor.lockState = CursorLockMode.None;
//        Cursor.visible = true;
//    }
//    void SwitchToGameplayMap() {
//        if (playerInput != null) playerInput.SwitchCurrentActionMap(gameplayMapName);

//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }
//    #endregion

//}