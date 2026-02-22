using System;
using UnityEngine;

public class FishingMinigameController : MonoBehaviour {
    #region Variables
    [Header("References")]
    [SerializeField] MinigameInputRouter inputRouter;
    [SerializeField] MinigameUIView uiView;
    [SerializeField] AudioSource minigameAudioSource;

    [Header("Settings")]
    [SerializeField] FishingMinigameSettings settings;


    bool hasZoneState;
    bool lastInZone;
    MinigameInput lastInput;

    AudioClip currentMinigameClip;


    IMinigame activeMinigame;
    MinigameContext context;
    MinigameResult result;

    public bool IsRunning { get { return activeMinigame != null && !activeMinigame.IsComplete; } }
    public MinigameResult LastResult { get { return result; } }

    public event Action<MinigameResult> OnMinigameEnded;
    #endregion

    #region Unity Methods
    void Update() {
        if (activeMinigame == null) return;

        MinigameInput input = inputRouter.ConsumeFrameInput();
        lastInput = input;

        if (input.cancelDown) {
            activeMinigame.End(MinigameEndReason.Cancelled);
        }
        else {
            activeMinigame.HandleInput(in input);
            activeMinigame.Tick(Time.deltaTime);
        }

        Render();

        if (activeMinigame.IsComplete) {
            FinishActiveMinigame();
        }
    }
    #endregion

    #region Utility Methods
    public void StartSpinningRodMinigame(FishSpeciesConfig species, float fishSize01, MinigameDifficulty difficulty, bool extendedFight) {
        StartMinigame(MinigameType.SpinningRod, species, fishSize01, difficulty, extendedFight);
    }

    public void StartCastingRodMinigame(FishSpeciesConfig species, float fishSize01, MinigameDifficulty difficulty, bool extendedFight) {
        StartMinigame(MinigameType.CastingRod, species, fishSize01, difficulty, extendedFight);
    }
    //DEPRECATED
    public void StartSpinningRodMinigame(MinigameDifficulty difficulty, bool extendedFight) {
        StartMinigame(MinigameType.SpinningRod, null, 0.5f, difficulty, extendedFight);
    }
    //DEPRECATED
    public void StartCastingRodMinigame(MinigameDifficulty difficulty, bool extendedFight) {
        StartMinigame(MinigameType.CastingRod, null, 0.5f, difficulty, extendedFight);
    }

    void StartMinigame(MinigameType type, FishSpeciesConfig species, float fishSize01, MinigameDifficulty difficulty, bool extendedFight) {
        if (settings == null) { Debug.LogError("FishingMinigameSettings is missing."); return; }
        if (inputRouter == null) { Debug.LogError("MinigameInputRouter is missing."); return; }
        if (uiView == null) { Debug.LogError("MinigameUIView is missing."); return; }

        StopMinigame(MinigameEndReason.Cancelled);

        fishSize01 = Mathf.Clamp01(fishSize01);

        context = new MinigameContext();
        context.type = type;
        context.difficulty = difficulty;
        context.extendedFight = extendedFight;

        // Defaults -> baked per species
        if (type == MinigameType.SpinningRod) {
            context.spinning = BakeSpinningSettings(settings.spinningDefaults, species, fishSize01);
        }
        else {
            context.spinning = settings.spinningDefaults;
        }

        //TODO
        context.casting = settings.castingDefaults;

        if (type == MinigameType.SpinningRod) {
            activeMinigame = new SpinningRodMinigame();
            uiView.ShowSpinning(true);
            uiView.ShowCasting(false);
        }
        else {
            activeMinigame = new CastingRodMinigame();
            uiView.ShowSpinning(false);
            uiView.ShowCasting(true);
        }

        inputRouter.EnableMinigameInput(true);

        hasZoneState = false;
        lastInZone = false;

        activeMinigame.Begin(in context);
    }

    public void StopMinigame(MinigameEndReason reason) {
        if (activeMinigame == null) return;

        if (!activeMinigame.IsComplete) {
            activeMinigame.End(reason);
        }

        FinishActiveMinigame();
    }

    void FinishActiveMinigame() {
        if (activeMinigame == null) return;

        result = activeMinigame.Result;

        inputRouter.EnableMinigameInput(false);

        uiView.ShowSpinning(false);
        uiView.ShowCasting(false);

        activeMinigame = null;

        hasZoneState = false;
        lastInZone = false;

        minigameAudioSource.Stop();

        OnMinigameEnded?.Invoke(result);

        Debug.Log($"Minigame ended. Reason: {result.reason} Land: {result.land01:0.00} Slack: {result.slack01:0.00}");
    }

    SpinningRodSettings BakeSpinningSettings(SpinningRodSettings baseSettings, FishSpeciesConfig species, float fishSize01) {
        SpinningRodSettings spinSettings = baseSettings;
        if (species == null) return spinSettings;

        // Species multipliers
        float timeMul = Mathf.Max(0.1f, species.spinning.timeMultiplier);
        float arcMul = Mathf.Max(0.1f, species.spinning.arcMultiplier);
        float landMul = Mathf.Max(0.1f, species.spinning.landGainMultiplier);

        float slackFillMul = Mathf.Max(0.1f, species.spinning.slackFillMultiplier);
        float slackDecayMul = Mathf.Max(0.1f, species.spinning.slackDecayMultiplier);

        float steerMul = Mathf.Max(0.1f, species.spinning.steeringMultiplier);

        // Size curves
        timeMul *= species.sizeScaling.timeBySize.Evaluate(fishSize01);
        arcMul *= species.sizeScaling.arcBySize.Evaluate(fishSize01);
        slackFillMul *= species.sizeScaling.slackBySize.Evaluate(fishSize01);
        landMul *= species.sizeScaling.landBySize.Evaluate(fishSize01);

        // Time (all tiers)
        spinSettings.easySeconds *= timeMul;
        spinSettings.mediumSeconds *= timeMul;
        spinSettings.hardSeconds *= timeMul;

        spinSettings.easySecondsExtended *= timeMul;
        spinSettings.mediumSecondsExtended *= timeMul;
        spinSettings.hardSecondsExtended *= timeMul;

        spinSettings.easySeconds = Mathf.Max(1f, spinSettings.easySeconds);
        spinSettings.mediumSeconds = Mathf.Max(1f, spinSettings.mediumSeconds);
        spinSettings.hardSeconds = Mathf.Max(1f, spinSettings.hardSeconds);

        spinSettings.easySecondsExtended = Mathf.Max(1f, spinSettings.easySecondsExtended);
        spinSettings.mediumSecondsExtended = Mathf.Max(1f, spinSettings.mediumSecondsExtended);
        spinSettings.hardSecondsExtended = Mathf.Max(1f, spinSettings.hardSecondsExtended);

        // Arc
        spinSettings.arcFillEasy = Mathf.Clamp(spinSettings.arcFillEasy * arcMul, 0.05f, 0.5f);
        spinSettings.arcFillMedium = Mathf.Clamp(spinSettings.arcFillMedium * arcMul, 0.05f, 0.5f);
        spinSettings.arcFillHard = Mathf.Clamp(spinSettings.arcFillHard * arcMul, 0.05f, 0.5f);

        // Relocations
        int add = species.spinning.relocationBonus;
        spinSettings.relocationsEasy = Mathf.Max(0, spinSettings.relocationsEasy + add);
        spinSettings.relocationsMedium = Mathf.Max(0, spinSettings.relocationsMedium + add);
        spinSettings.relocationsHard = Mathf.Max(0, spinSettings.relocationsHard + add);

        // Land
        spinSettings.landGainPerSecond = Mathf.Max(0.01f, spinSettings.landGainPerSecond * landMul);

        // Slack
        spinSettings.slackBaseFillPerSecond = Mathf.Max(0.01f, spinSettings.slackBaseFillPerSecond * slackFillMul);
        spinSettings.slackDistanceFillPerSecond = Mathf.Max(0.01f, spinSettings.slackDistanceFillPerSecond * slackFillMul);
        spinSettings.slackDecayPerSecond = Mathf.Max(0.01f, spinSettings.slackDecayPerSecond * slackDecayMul);

        // Steering
        spinSettings.degreesPerPixel *= steerMul;
        spinSettings.maxCursorDegPerSecond *= steerMul;

        return spinSettings;
    }

    void Render() {
        if (activeMinigame == null) return;
        if (activeMinigame.Type == MinigameType.SpinningRod) {
            SpinningRodMinigame spinning = activeMinigame as SpinningRodMinigame;

            if (spinning == null) return;

            SpinningRodMinigame.State s = spinning.GetState();
            HandleSpinningZoneSFX(s.inZone);
            uiView.RenderSpinning(s);
        }
        else {
            CastingRodMinigame casting = activeMinigame as CastingRodMinigame;
            if (casting == null) return;

            CastingRodMinigame.State s = casting.GetState();

            bool isHolding = lastInput.primaryHeld && lastInput.isRightSide;
            HandleCastingLoopSFX(s.isSurging, isHolding);

            uiView.RenderCasting(s);
        }
    }

    void HandleSpinningZoneSFX(bool inZone) {
        if (!hasZoneState) {
            hasZoneState = true;
            lastInZone = inZone;
            return;
        }

        if (inZone == lastInZone) return;

        lastInZone = inZone;

        AudioClip clip = inZone ? settings.spinningDefaults.spinningRodLandSound : settings.spinningDefaults.spinningRodSlackSound;
        if (clip == null) return;

        minigameAudioSource.clip = clip;
        minigameAudioSource.Play();
    }

    void HandleCastingLoopSFX(bool isSurging, bool isHolding) {
        AudioClip clip = minigameAudioSource.clip;

        if (isHolding) {
            if (clip == settings.castingDefaults.castingHoldSound) return;
            clip = settings.castingDefaults.castingHoldSound;
        }
        else {
            if (isSurging) {
                if (clip == settings.castingDefaults.castingLandSurgeSound) return;
                clip = settings.castingDefaults.castingLandSurgeSound;
            }
            else {
                if (clip == settings.castingDefaults.castingLandNoSurgeSound) return;
                clip = settings.castingDefaults.castingLandNoSurgeSound;
            }
        }
        minigameAudioSource.Stop();
        minigameAudioSource.clip = clip;
        minigameAudioSource.Play();
    }

    #endregion

}
