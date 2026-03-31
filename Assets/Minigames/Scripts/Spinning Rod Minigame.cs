using UnityEngine;

public class SpinningRodMinigame : IMinigame {
    #region Variables
    MinigameContext ctx;
    SpinningRodSettings settings;

    float timeRemaining;
    float totalTime;

    float land;
    float slack;

    float cursorDeg;
    float cursorVelDegPerSec;

    float targetCenterDeg;
    float arcFill01;
    float arcHalfDeg;

    int relocationsRemaining;
    float nextRelocationTime;
    float relocationGraceRemaining;

    bool complete;
    MinigameResult result;

    MinigameInput input;


    public MinigameType Type { get { return MinigameType.SpinningRod; } }
    public bool IsComplete { get { return complete; } }
    public MinigameResult Result { get { return result; } }
    #endregion

    #region Interface Methods
    public void Begin(in MinigameContext context) {
        //Helpers
        ctx = context;
        settings = context.spinning;

        //Initialize
        totalTime = ResolveTimeSeconds(ctx.difficulty, ctx.extendedFight, settings);
        timeRemaining = totalTime;
        arcFill01 = ResolveArcFill(ctx.difficulty, settings);
        arcHalfDeg = (arcFill01 * 360f) * 0.5f;

        land = 0f;
        slack = 0f;

        targetCenterDeg = Random.Range(0f, 360f);

        //position cursor
        float startFactor = 1.5f; // default
        if (ctx.difficulty == MinigameDifficulty.Medium) startFactor = 1.75f;
        if (ctx.difficulty == MinigameDifficulty.Hard) startFactor = 2f; // can be outside

        float range = arcHalfDeg * startFactor;
        float startOffset = Random.Range(arcHalfDeg, range);

        cursorDeg = Wrap360(targetCenterDeg + startOffset);
        cursorVelDegPerSec = 0f;

        relocationsRemaining = ResolveRelocations(ctx.difficulty, ctx.extendedFight, settings);

        nextRelocationTime = ComputeNextRelocationTime(timeRemaining, relocationsRemaining);

        relocationGraceRemaining = 0f;

        complete = false;
        result = new MinigameResult { reason = MinigameEndReason.None, land01 = 0f, slack01 = 0f };

        Debug.Log($"Game started: \n\tArc size: {arcFill01 * 360f} degrees\n\tArc center location: {targetCenterDeg}\n\tRelocations: {relocationsRemaining}\n\tNext relocation in: {nextRelocationTime}s");
        Debug.Log($"Cursor Location: {cursorDeg}");
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

        // Relocation grace countdown
        if (relocationGraceRemaining > 0f) {
            relocationGraceRemaining -= deltaTime;
            if (relocationGraceRemaining < 0f) relocationGraceRemaining = 0f;
        }

        //bumper
        float deltaTimeSafe = deltaTime > 0.0001f ? deltaTime : 0.016f;
        float targetVel = (input.dragDelta.x / deltaTimeSafe) * settings.degreesPerPixel;

        // Clamp
        float maxVel = Mathf.Max(10f, settings.maxCursorDegPerSecond);
        targetVel = Mathf.Clamp(targetVel, -maxVel, maxVel);

        // Smooth
        float smooth = Mathf.Clamp01(settings.steeringSmoothing);
        cursorVelDegPerSec = Mathf.Lerp(
            cursorVelDegPerSec,
            targetVel,
            1f - Mathf.Pow(1f - smooth, 60f * deltaTimeSafe)
        );

        // Integrate angle
        cursorDeg = Wrap360(cursorDeg + cursorVelDegPerSec * deltaTime);

        // Relocation scheduling
        if (relocationsRemaining > 0 && timeRemaining <= nextRelocationTime) {
            RelocateTarget();
        }

        // Compute zone & distance
        float absDeltaToTargetCenter = GetAbsAngularDelta(cursorDeg, targetCenterDeg);
        bool inZone = absDeltaToTargetCenter <= arcHalfDeg;

        // Land meter
        if (inZone) {
            float gain = settings.landGainPerSecond;

            land += gain * deltaTime;
            if (land > 1f) land = 1f;
        }
        else {
            land -= settings.landDecayPerSecond * deltaTime;
            if (land < 0f) land = 0f;
        }

        // Slack meter (grace right after relocations)
        if (inZone) {
            slack -= settings.slackDecayPerSecond * deltaTime;
            if (slack < 0f) slack = 0f;
        }
        else {
            // Normalize distance
            float maxDist = Mathf.Max(1f, settings.maxRelevantDistanceDeg);
            float normalizedDistance = Mathf.Clamp01(absDeltaToTargetCenter / maxDist);
            float curveIncreasePower = Mathf.Max(1f, settings.slackDistancePower);

            float fillRate = settings.slackBaseFillPerSecond +
                             (settings.slackDistanceFillPerSecond * Mathf.Pow(normalizedDistance, curveIncreasePower));

            // Apply grace reduction after relocation
            if (relocationGraceRemaining > 0f) {
                float graceTime = 1f - (relocationGraceRemaining / Mathf.Max(0.01f, settings.relocationGraceSeconds));
                float graceScale = Mathf.Lerp(0.5f, 1f, graceTime);
                fillRate *= graceScale;
            }

            slack += fillRate * deltaTime;
            if (slack > 1f) slack = 1f;
        }

        // Result checks
        if (land >= 1f) {
            End(MinigameEndReason.Success);
            return;
        }

        if (slack >= 1f) {
            End(MinigameEndReason.SlackMaxed);
            return;
        }
    }

    public void End(MinigameEndReason reason) {
        if (complete) return;

        complete = true;

        CatchInfo? catchInfo = null;

        if (reason == MinigameEndReason.Success) {
            float length = 0f;
            float weight = 0f;
            if (ctx.species != null)
                ctx.species.TryResolveLengthWeight(ctx.fishSize01, out length, out weight);
            catchInfo = new CatchInfo {
                species = ctx.species,
                fishSize01 = ctx.fishSize01,
                isTrophy = ctx.species.isTrophy,
                length = length,
                weight = weight
            };
        }

        result = new MinigameResult {
            reason = reason,
            land01 = land,
            slack01 = slack,
            catchInfo = catchInfo
        };
    }
    #endregion

    #region Utility Methods
    public State GetState() {
        State s = new State();

        s.timeRemainingSeconds = timeRemaining;
        s.timeRemaining01 = totalTime > 0f ? Mathf.Clamp01(timeRemaining / totalTime) : 0f;

        s.land01 = land;
        s.slack01 = slack;

        s.targetCenterDeg = targetCenterDeg;
        s.arcFill01 = arcFill01;
        s.cursorDeg = cursorDeg;

        s.inZone = GetAbsAngularDelta(cursorDeg, targetCenterDeg) <= arcHalfDeg;

        return s;
    }

    float ResolveTimeSeconds(MinigameDifficulty diff, bool extended, SpinningRodSettings s) {
        if (extended) {
            if (diff == MinigameDifficulty.Easy) return s.easySecondsExtended;
            if (diff == MinigameDifficulty.Medium) return s.mediumSecondsExtended;
            return s.hardSecondsExtended;
        }

        if (diff == MinigameDifficulty.Easy) return s.easySeconds;
        if (diff == MinigameDifficulty.Medium) return s.mediumSeconds;
        return s.hardSeconds;
    }

    float ResolveArcFill(MinigameDifficulty diff, SpinningRodSettings s) {
        if (diff == MinigameDifficulty.Easy) return s.arcFillEasy;
        if (diff == MinigameDifficulty.Medium) return s.arcFillMedium;
        return s.arcFillHard;
    }

    int ResolveRelocations(MinigameDifficulty diff, bool extended, SpinningRodSettings s) {
        int baseRelocs;
        if (diff == MinigameDifficulty.Easy) baseRelocs = s.relocationsEasy;
        else if (diff == MinigameDifficulty.Medium) baseRelocs = s.relocationsMedium;
        else baseRelocs = s.relocationsHard;

        if (extended) baseRelocs += 1; //Extended fights get an extra relocation

        if (baseRelocs < 0) baseRelocs = 0;
        return baseRelocs;
    }

    float ComputeNextRelocationTime(float timeRemainingSeconds, int remainingRelocations) {
        if (remainingRelocations <= 0) return -1f;

        float slice = timeRemainingSeconds / (remainingRelocations + 1);
        float next = timeRemainingSeconds - slice;

        if (next < 0.25f) next = 0.25f;
        return next;
    }

    void RelocateTarget() {
        targetCenterDeg = Random.Range(0f, 360f);

        relocationsRemaining -= 1;
        if (relocationsRemaining < 0) relocationsRemaining = 0;

        relocationGraceRemaining = Mathf.Max(0f, settings.relocationGraceSeconds);

        nextRelocationTime = ComputeNextRelocationTime(timeRemaining, relocationsRemaining);
    }

    float GetAbsAngularDelta(float aDeg, float bDeg) {
        float delta = Mathf.DeltaAngle(aDeg, bDeg);
        if (delta < 0f) delta = -delta;
        return delta;
    }

    float Wrap360(float deg) {
        deg %= 360f;
        if (deg < 0f) deg += 360f;
        return deg;
    }
    #endregion

    #region Structs
    public struct State {
        public float timeRemainingSeconds;
        public float timeRemaining01;

        public float land01;
        public float slack01;

        public float targetCenterDeg;   // 0..360
        public float arcFill01;         // 0..1 (of 360)
        public float cursorDeg;         // 0..360

        public bool inZone;
    }

    #endregion
}
