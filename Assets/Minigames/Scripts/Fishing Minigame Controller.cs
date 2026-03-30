using System;
using UnityEngine;

public class FishingMinigameController : MonoBehaviour {
    #region Variables
    [Header("References")]
    [SerializeField] MinigameInputRouter inputRouter;
    [SerializeField] MinigameUIView uiView;

    [Header("Audio")]
    [SerializeField] AudioSource minigameAudioSource;
    [SerializeField] AudioClip successClip;
    [SerializeField] AudioClip failureClip;

    [Header("Settings")]
    [SerializeField] FishingMinigameSettings settings;

    bool hasZoneState;
    bool lastInZone;
    MinigameInput lastInput;

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
        StartMinigame(MinigameType.SpinningRod, species, fishSize01, difficulty, extendedFight, TackleModifiers.Default());
    }

    public void StartCastingRodMinigame(FishSpeciesConfig species, float fishSize01, MinigameDifficulty difficulty, bool extendedFight) {
        StartMinigame(MinigameType.CastingRod, species, fishSize01, difficulty, extendedFight, TackleModifiers.Default());
    }

    public void StartSpinningRodMinigame(FishSpeciesConfig species, float fishSize01, MinigameDifficulty difficulty, bool extendedFight, TackleModifiers tackle) {
        StartMinigame(MinigameType.SpinningRod, species, fishSize01, difficulty, extendedFight, tackle);
    }

    public void StartCastingRodMinigame(FishSpeciesConfig species, float fishSize01, MinigameDifficulty difficulty, bool extendedFight, TackleModifiers tackle) {
        StartMinigame(MinigameType.CastingRod, species, fishSize01, difficulty, extendedFight, tackle);
    }

    //// DEPRECATED
    //public void StartSpinningRodMinigame(MinigameDifficulty difficulty, bool extendedFight) {
    //    StartMinigame(MinigameType.SpinningRod, null, 0.5f, difficulty, extendedFight, TackleModifiers.Identity());
    //}

    //// DEPRECATED
    //public void StartCastingRodMinigame(MinigameDifficulty difficulty, bool extendedFight) {
    //    StartMinigame(MinigameType.CastingRod, null, 0.5f, difficulty, extendedFight, TackleModifiers.Identity());
    //}

    void StartMinigame(MinigameType type, FishSpeciesConfig species, float fishSize01, MinigameDifficulty difficulty, bool extendedFight, TackleModifiers tackle) {
        if (settings == null) { Debug.LogError("FishingMinigameSettings is missing."); return; }
        if (inputRouter == null) { Debug.LogError("MinigameInputRouter is missing."); return; }
        if (uiView == null) { Debug.LogError("MinigameUIView is missing."); return; }

        StopMinigame(MinigameEndReason.Cancelled);

        fishSize01 = Mathf.Clamp01(fishSize01);

        context = new MinigameContext();
        context.type = type;
        context.difficulty = difficulty;
        context.extendedFight = extendedFight;
        context.species = species;
        context.fishSize01 = fishSize01;

        if (type == MinigameType.SpinningRod) {
            SpinningRodSettings spinningSettings = BakeSpinningSettings(settings.spinningDefaults, species, fishSize01);
            spinningSettings = ApplyTackleToSpinning(spinningSettings, tackle);
            context.spinning = spinningSettings;

            context.casting = settings.castingDefaults;
        }
        else {
            CastingRodSettings castingSettings = BakeCastingSettings(settings.castingDefaults, species, fishSize01, extendedFight);
            castingSettings = ApplyTackleToCasting(castingSettings, tackle);
            context.casting = castingSettings;

            context.spinning = settings.spinningDefaults;
        }

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

        if (minigameAudioSource != null) minigameAudioSource.Stop();

        if (result.reason == MinigameEndReason.Success) {
            HandleMinigameSuccessSFX();
        }
        else {
            HandleMinigameFailSFX();
        }

        OnMinigameEnded?.Invoke(result);

        Debug.Log($"Minigame ended. Reason: {result.reason} Land: {result.land01:0.00} Slack: {result.slack01:0.00}");
        if (result.reason == MinigameEndReason.Success) {
            CatchInfo catchInfo = result.catchInfo.Value;
            float length = 0;
            float width = 0;
            catchInfo.species.TryResolveLengthWeight(catchInfo.fishSize01, out length, out width);

            Debug.Log($"Fish caught:\n{catchInfo.species.name}\n\tLength: {length.ToString("F2")} inches\n\tWidth: {width.ToString("F2")} lbs");
        }
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

        // Time
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

    CastingRodSettings BakeCastingSettings(CastingRodSettings baseSettings, FishSpeciesConfig species, float fishSize01, bool extendedFight) {
        CastingRodSettings castSettings = baseSettings;
        if (species == null) {
            // Still apply extended pulses
            if (extendedFight) {
                castSettings.pulseCountEasy = Mathf.Max(0, castSettings.pulseCountEasy + castSettings.pulseBonusExtended);
                castSettings.pulseCountMedium = Mathf.Max(0, castSettings.pulseCountMedium + castSettings.pulseBonusExtended);
                castSettings.pulseCountHard = Mathf.Max(0, castSettings.pulseCountHard + castSettings.pulseBonusExtended);
            }
            return castSettings;
        }

        // Species multipliers
        float timeMul = Mathf.Max(0.1f, species.casting.timeMultiplier);

        float heatStartMul = Mathf.Max(0.1f, species.casting.heatStartMultiplier);
        float backlashMul = Mathf.Max(0.1f, species.casting.backlashThresholdMultiplier);

        float heatRiseMul = Mathf.Max(0.1f, species.casting.heatRiseMultiplier);
        float heatFallHoldMul = Mathf.Max(0.1f, species.casting.heatFallHoldMultiplier);
        float heatFallHoldSurgeMul = Mathf.Max(0.1f, species.casting.heatFallHoldSurgeMultiplier);

        float landGainCalmMul = Mathf.Max(0.1f, species.casting.landGainNoHoldCalmMultiplier);
        float landGainSurgeMul = Mathf.Max(0.1f, species.casting.landGainNoHoldSurgeMultiplier);
        float landLossHoldSurgeMul = Mathf.Max(0.1f, species.casting.landLossHoldSurgeMultiplier);

        float surgeDurationMul = Mathf.Max(0.1f, species.casting.surgeDurationMultiplier);
        float pulseJitterMul = Mathf.Max(0.1f, species.casting.pulseJitterMultiplier);
        float pulseMarginMul = Mathf.Max(0.1f, species.casting.pulseStartEndMarginMultiplier);

        // Size scaling
        float heatBySize = 1f;
        if (species.sizeScaling.heatBySize != null) {
            heatBySize = Mathf.Max(0.1f, species.sizeScaling.heatBySize.Evaluate(fishSize01));
        }

        int pulseBySizeAdd = 0;
        if (species.sizeScaling.pulseBySize != null) {
            pulseBySizeAdd = Mathf.RoundToInt(species.sizeScaling.pulseBySize.Evaluate(fishSize01));
        }

        // Time
        castSettings.easySeconds *= timeMul;
        castSettings.mediumSeconds *= timeMul;
        castSettings.hardSeconds *= timeMul;

        castSettings.easySecondsExtended *= timeMul;
        castSettings.mediumSecondsExtended *= timeMul;
        castSettings.hardSecondsExtended *= timeMul;

        castSettings.easySeconds = Mathf.Max(1f, castSettings.easySeconds);
        castSettings.mediumSeconds = Mathf.Max(1f, castSettings.mediumSeconds);
        castSettings.hardSeconds = Mathf.Max(1f, castSettings.hardSeconds);

        castSettings.easySecondsExtended = Mathf.Max(1f, castSettings.easySecondsExtended);
        castSettings.mediumSecondsExtended = Mathf.Max(1f, castSettings.mediumSecondsExtended);
        castSettings.hardSecondsExtended = Mathf.Max(1f, castSettings.hardSecondsExtended);

        // Heat start / threshold
        castSettings.heatStart01 = Mathf.Clamp01(castSettings.heatStart01 * heatStartMul);
        castSettings.backlashThreshold01 = Mathf.Clamp(castSettings.backlashThreshold01 * backlashMul, 0.05f, 1f);

        // Heat rates 
        castSettings.heatRiseNoHoldCalmPerSecond = Mathf.Max(0.01f, castSettings.heatRiseNoHoldCalmPerSecond * heatRiseMul * heatBySize);
        castSettings.heatRiseNoHoldSurgePerSecond = Mathf.Max(0.01f, castSettings.heatRiseNoHoldSurgePerSecond * heatRiseMul * heatBySize);
        castSettings.heatFallHoldPerSecond = Mathf.Max(0.01f, castSettings.heatFallHoldPerSecond * heatFallHoldMul);
        castSettings.heatFallHoldSurgeMultiplier = Mathf.Clamp(castSettings.heatFallHoldSurgeMultiplier * heatFallHoldSurgeMul, 0.1f, 2f);

        // Land rates
        castSettings.landGainNoHoldCalmPerSecond = Mathf.Max(0.01f, castSettings.landGainNoHoldCalmPerSecond * landGainCalmMul);
        castSettings.landGainNoHoldSurgePerSecond = Mathf.Max(0.01f, castSettings.landGainNoHoldSurgePerSecond * landGainSurgeMul);
        castSettings.landLossHoldSurgePerSecond = Mathf.Max(0.01f, castSettings.landLossHoldSurgePerSecond * landLossHoldSurgeMul);

        // Pulses
        int pulseAdd = species.casting.pulseBonus + pulseBySizeAdd;
        castSettings.pulseCountEasy = Mathf.Max(0, castSettings.pulseCountEasy + pulseAdd);
        castSettings.pulseCountMedium = Mathf.Max(0, castSettings.pulseCountMedium + pulseAdd);
        castSettings.pulseCountHard = Mathf.Max(0, castSettings.pulseCountHard + pulseAdd);

        if (extendedFight) {
            castSettings.pulseCountEasy = Mathf.Max(0, castSettings.pulseCountEasy + castSettings.pulseBonusExtended);
            castSettings.pulseCountMedium = Mathf.Max(0, castSettings.pulseCountMedium + castSettings.pulseBonusExtended);
            castSettings.pulseCountHard = Mathf.Max(0, castSettings.pulseCountHard + castSettings.pulseBonusExtended);
        }

        // Surge timing
        castSettings.surgeDurationSeconds = Mathf.Max(0.05f, castSettings.surgeDurationSeconds * surgeDurationMul);
        castSettings.pulseJitterSeconds = Mathf.Max(0f, castSettings.pulseJitterSeconds * pulseJitterMul);
        castSettings.pulseStartEndMarginSeconds = Mathf.Max(0f, castSettings.pulseStartEndMarginSeconds * pulseMarginMul);

        return castSettings;
    }

    SpinningRodSettings ApplyTackleToSpinning(SpinningRodSettings spinningSettings, TackleModifiers tackle) {
        spinningSettings.landGainPerSecond = Mathf.Max(0.01f, spinningSettings.landGainPerSecond * Mathf.Max(0.05f, tackle.spinningLandGainMultiplier));

        float slackGainMul = Mathf.Max(0.05f, tackle.spinningSlackGainMultiplier);
        spinningSettings.slackBaseFillPerSecond = Mathf.Max(0.01f, spinningSettings.slackBaseFillPerSecond * slackGainMul);
        spinningSettings.slackDistanceFillPerSecond = Mathf.Max(0.01f, spinningSettings.slackDistanceFillPerSecond * slackGainMul);

        spinningSettings.slackDecayPerSecond = Mathf.Max(0.01f, spinningSettings.slackDecayPerSecond * Mathf.Max(0.05f, tackle.spinningSlackDecayMultiplier));
        return spinningSettings;
    }

    CastingRodSettings ApplyTackleToCasting(CastingRodSettings castingSettings, TackleModifiers tackle) {
        float heatGainMul = Mathf.Max(0.05f, tackle.castingHeatGainMultiplier);
        float heatDecayMul = Mathf.Max(0.05f, tackle.castingHeatDecayMultiplier);

        castingSettings.heatRiseNoHoldCalmPerSecond = Mathf.Max(0.01f, castingSettings.heatRiseNoHoldCalmPerSecond * heatGainMul);
        castingSettings.heatRiseNoHoldSurgePerSecond = Mathf.Max(0.01f, castingSettings.heatRiseNoHoldSurgePerSecond * heatGainMul);

        castingSettings.heatFallHoldPerSecond = Mathf.Max(0.01f, castingSettings.heatFallHoldPerSecond * heatDecayMul);

        castingSettings.landGainNoHoldCalmPerSecond = Mathf.Max(0.01f, castingSettings.landGainNoHoldCalmPerSecond * Mathf.Max(0.05f, tackle.castingLandGainMultiplier));
        castingSettings.landGainNoHoldSurgePerSecond = Mathf.Max(0.01f, castingSettings.landGainNoHoldSurgePerSecond * Mathf.Max(0.05f, tackle.castingLandGainMultiplier));
        castingSettings.landLossHoldSurgePerSecond = Mathf.Max(0.01f, castingSettings.landLossHoldSurgePerSecond * Mathf.Max(0.05f, tackle.castingLandLossMultiplier));

        return castingSettings;
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
        minigameAudioSource.loop = true;
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
        minigameAudioSource.loop = true;
        minigameAudioSource.Play();
    }

    void HandleMinigameSuccessSFX() {
        AudioClip clip = successClip;

        minigameAudioSource.Stop();
        minigameAudioSource.clip = clip;
        minigameAudioSource.loop = false;
        minigameAudioSource.Play();
    }

    void HandleMinigameFailSFX() {
        AudioClip clip = failureClip;

        minigameAudioSource.Stop();
        minigameAudioSource.clip = clip;
        minigameAudioSource.loop = false;
        minigameAudioSource.Play();
    }

    #endregion
}