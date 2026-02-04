using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Minigames/Fishing Minigame Settings", fileName = "FishingMinigameSettings")]
public class FishingMinigameSettings : ScriptableObject {
    public SpinningRodSettings spinningDefaults;
    public CastingRodSettings castingDefaults;
}

#region Structs
[Serializable]
public struct SpinningRodSettings {
    [Header("Timer (seconds)")]
    public float easySeconds;
    public float mediumSeconds;
    public float hardSeconds;

    [Header("Extended Timer (seconds)")]
    public float easySecondsExtended;
    public float mediumSecondsExtended;
    public float hardSecondsExtended;

    [Header("Arc")]
    [Range(0.05f, 0.5f)] public float arcFillEasy;   // fill amount of 360 (0..1)
    [Range(0.05f, 0.5f)] public float arcFillMedium;
    [Range(0.05f, 0.5f)] public float arcFillHard;

    [Header("Relocations")]
    public int relocationsEasy;
    public int relocationsMedium;
    public int relocationsHard;
    [Range(0f, 1f)] public float relocationGraceSeconds;

    [Header("Land")]
    [Range(0.01f, 2f)] public float landGainPerSecond;
    [Range(0.01f, 2f)] public float landDecayPerSecond;

    [Header("Slack")]
    [Range(0.01f, 2f)] public float slackDecayPerSecond;
    [Range(0.01f, 2f)] public float slackBaseFillPerSecond;
    [Range(0.01f, 2f)] public float slackDistanceFillPerSecond;
    [Range(1f, 3f)] public float slackDistancePower;
    [Range(30f, 180f)] public float maxRelevantDistanceDeg;

    [Header("Steering")]
    public float degreesPerPixel;
    public float maxCursorDegPerSecond;
    [Range(0f, 1f)] public float steeringSmoothing; // 0 = no smoothing, 1 = heavy smoothing

    [Header("SFX")]
    public AudioClip spinningRodLandSound;
    public AudioClip spinningRodSlackSound;
}

[Serializable]
public struct CastingRodSettings {
    [Header("Timer (seconds)")]
    public float easySeconds;
    public float mediumSeconds;
    public float hardSeconds;

    [Header("Extended Timer (seconds)")]
    public float easySecondsExtended;
    public float mediumSecondsExtended;
    public float hardSecondsExtended;

    [Header("Heat")]
    [Range(0f, 1f)] public float heatStart01;
    [Range(0.05f, 1f)] public float backlashThreshold01;
    [Range(0.01f, 4f)] public float heatRiseNoHoldCalmPerSecond;
    [Range(0.01f, 6f)] public float heatRiseNoHoldSurgePerSecond;
    [Range(0.01f, 6f)] public float heatFallHoldPerSecond;
    [Range(0.1f, 1f)] public float heatFallHoldSurgeMultiplier; // For trophy

    [Header("Land Rates (per second)")]
    [Range(0.01f, 2f)] public float landGainNoHoldCalmPerSecond;
    [Range(0.01f, 3f)] public float landGainNoHoldSurgePerSecond;
    [Range(0.01f, 3f)] public float landLossHoldSurgePerSecond;


    [Header("Surge Pulses")]
    public int pulseCountEasy;
    public int pulseCountMedium;
    public int pulseCountHard;

    [Header("Extended Fight")]
    public int pulseBonusExtended;

    [Header("Surge Timing")]
    [Range(0.1f, 2f)] public float surgeDurationSeconds;
    [Range(0f, 2f)] public float pulseJitterSeconds;            // randomness on pulses
    [Range(0f, 2f)] public float pulseStartEndMarginSeconds;    // keep pulses off the very start/end

    [Header("SFX")]
    public AudioClip castingLandNoSurgeSound;
    public AudioClip castingLandSurgeSound;
    public AudioClip castingHoldSound;
}
#endregion
