using UnityEngine;

public class CastingRodMinigame : IMinigame {
    #region Variables
    MinigameContext ctx;
    CastingRodSettings settings;

    float timeRemaining;
    float totalTime;

    float land;
    float heat;                 // raw heat value (0..backlashThreshold01)
    float backlashThreshold;

    int pulsesRemaining;
    float nextPulseTimeRemaining;
    float surgeRemaining;

    bool complete;
    MinigameResult result;

    MinigameInput input;

    public MinigameType Type { get { return MinigameType.CastingRod; } }
    public bool IsComplete { get { return complete; } }
    public MinigameResult Result { get { return result; } }
    #endregion

    #region Interface Methods
    public void Begin(in MinigameContext context) {
        ctx = context;
        settings = context.casting;

        totalTime = ResolveTimeSeconds(ctx.difficulty, ctx.extendedFight, settings);
        timeRemaining = totalTime;

        land = 0f;

        backlashThreshold = Mathf.Clamp(settings.backlashThreshold01, 0.05f, 1f);
        heat = Mathf.Clamp(settings.heatStart01, 0f, backlashThreshold);

        pulsesRemaining = ResolvePulseCount(ctx.difficulty, ctx.extendedFight, settings);

        surgeRemaining = 0f;
        nextPulseTimeRemaining = ComputeNextPulseTime(timeRemaining, pulsesRemaining, settings);

        complete = false;
        result = new MinigameResult { reason = MinigameEndReason.None, land01 = 0f, slack01 = 0f };
    }

    public void HandleInput(in MinigameInput minigameInput) {
        input = minigameInput;
    }

    public void Tick(float deltaTime) {
        if (complete) return;

        // Timer
        timeRemaining -= deltaTime;
        if (timeRemaining <= 0f) {
            timeRemaining = 0f;
            End(MinigameEndReason.TimeOut);
            return;
        }

        // Pulse scheduling
        bool isSurging = surgeRemaining > 0f;

        if (!isSurging && pulsesRemaining > 0 && timeRemaining <= nextPulseTimeRemaining) {
            StartSurge();
            isSurging = true;
        }

        if (surgeRemaining > 0f) {
            surgeRemaining -= deltaTime;
            if (surgeRemaining < 0f) surgeRemaining = 0f;
            isSurging = surgeRemaining > 0f;
        }

        // HOLD INPUT (thumb on spool)
        // Rename `holdHeld` to match your MinigameInput field.
        bool holding = input.primaryHeld && input.isRightSide;

        // LAND + HEAT rules
        if (!isSurging) {
            // CALM PHASE
            if (holding) {
                // land stalls
                // heat cools
                heat -= settings.heatFallHoldPerSecond * deltaTime;
            }
            else {
                // land gains
                land += settings.landGainNoHoldCalmPerSecond * deltaTime;

                // heat rises gently
                heat += settings.heatRiseNoHoldCalmPerSecond * deltaTime;
            }
        }
        else {
            // SURGE PHASE
            if (holding) {
                // land loses (fish pulls line / you give ground)
                land -= settings.landLossHoldSurgePerSecond * deltaTime;

                // heat cools, but usually slower (trophy fish will bake this down)
                heat -= settings.heatFallHoldPerSecond * settings.heatFallHoldSurgeMultiplier * deltaTime;
            }
            else {
                // land gains (you fight it)
                land += settings.landGainNoHoldSurgePerSecond * deltaTime;

                // heat rises fast (backlash risk)
                heat += settings.heatRiseNoHoldSurgePerSecond * deltaTime;
            }
        }

        // Clamp
        land = Mathf.Clamp01(land);
        heat = Mathf.Clamp(heat, 0f, backlashThreshold);

        // result check
        if (land >= 1f) {
            End(MinigameEndReason.Success);
            return;
        }

        if (heat >= backlashThreshold) {
            End(MinigameEndReason.SlackMaxed); // reuse reason OR add Backlash if you created one
            return;
        }
    }

    public void End(MinigameEndReason reason) {
        if (complete) return;

        complete = true;

        result = new MinigameResult {
            reason = reason,
            land01 = land,
            // We reuse slack01 as "heat01" for casting in your existing MinigameResult.
            // This keeps the framework consistent without introducing a new result type.
            slack01 = GetHeat01()
        };
    }
    #endregion

    #region Utility Methods
    public State GetState() {
        State s = new State();

        s.timeRemainingSeconds = timeRemaining;
        s.timeRemaining01 = totalTime > 0f ? Mathf.Clamp01(timeRemaining / totalTime) : 0f;

        s.land01 = land;
        s.heat01 = GetHeat01();

        s.isSurging = surgeRemaining > 0f;
        s.surgeRemaining01 = settings.surgeDurationSeconds > 0.001f
            ? Mathf.Clamp01(surgeRemaining / settings.surgeDurationSeconds)
            : 0f;

        return s;
    }

    float GetHeat01() {
        if (backlashThreshold <= 0.0001f) return 0f;
        return Mathf.Clamp01(heat / backlashThreshold);
    }

    float ResolveTimeSeconds(MinigameDifficulty diff, bool extended, CastingRodSettings s) {
        if (extended) {
            if (diff == MinigameDifficulty.Easy) return s.easySecondsExtended;
            if (diff == MinigameDifficulty.Medium) return s.mediumSecondsExtended;
            return s.hardSecondsExtended;
        }

        if (diff == MinigameDifficulty.Easy) return s.easySeconds;
        if (diff == MinigameDifficulty.Medium) return s.mediumSeconds;
        return s.hardSeconds;
    }

    int ResolvePulseCount(MinigameDifficulty diff, bool extended, CastingRodSettings s) {
        int count;
        if (diff == MinigameDifficulty.Easy) count = s.pulseCountEasy;
        else if (diff == MinigameDifficulty.Medium) count = s.pulseCountMedium;
        else count = s.pulseCountHard;

        if (extended) count += Mathf.Max(0, s.pulseBonusExtended);

        if (count < 0) count = 0;
        return count;
    }

    float ComputeNextPulseTime(float timeRemainingSeconds, int remainingPulses, CastingRodSettings s) {
        if (remainingPulses <= 0) return -1f;

        // Evenly distributed pulses with a little jitter.
        // `timeRemainingSeconds` counts DOWN, so "next pulse timeRemaining" should be smaller than current.
        float margin = Mathf.Clamp(s.pulseStartEndMarginSeconds, 0f, 10f);
        float usable = Mathf.Max(0.25f, timeRemainingSeconds - margin * 2f);

        float slice = usable / (remainingPulses + 1);
        float baseNext = timeRemainingSeconds - margin - slice;

        float jitter = Mathf.Clamp(s.pulseJitterSeconds, 0f, slice * 0.45f);
        float offset = jitter > 0f ? Random.Range(-jitter, jitter) : 0f;

        float next = baseNext + offset;

        // Keep it sane
        float minTimeRemaining = Mathf.Max(0.25f, margin);
        if (next < minTimeRemaining) next = minTimeRemaining;

        return next;
    }

    void StartSurge() {
        surgeRemaining = Mathf.Max(0.05f, settings.surgeDurationSeconds);

        pulsesRemaining -= 1;
        if (pulsesRemaining < 0) pulsesRemaining = 0;

        nextPulseTimeRemaining = ComputeNextPulseTime(timeRemaining, pulsesRemaining, settings);
    }
    #endregion

    #region Structs
    public struct State {
        public float timeRemainingSeconds;
        public float timeRemaining01;

        public float land01;
        public float heat01;

        public bool isSurging;
        public float surgeRemaining01;
    }
    #endregion
}
