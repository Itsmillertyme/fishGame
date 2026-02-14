using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Fish Species Config", fileName = "FishSpeciesConfig")]
public class FishSpeciesConfig : ScriptableObject {
    [Header("Identity")]
    [Tooltip("Display name shown in the UI (Anglerpedia, catch screens, etc.).")]
    public string displayName;
    [Tooltip("Icon used for this species in the UI.")]
    public Sprite icon;
    [Tooltip("Short flavor text / description for the species.")]
    public string description;
    [Tooltip("The environments in which this species of fish can be found")]
    public List<Environment> environments;

    [Space(10)]
    [Tooltip("True if this is a Trophy variant of the fish.")]
    public bool isTrophy;



    [Header("Anglerpedia Stats")]
    [Tooltip("How hard the fish pulls overall (1 = easy, 5 = very strong).")]
    [Range(1, 5)]
    public int strength;
    [Tooltip("How often the fish fights / surges (1 = calm, 5 = aggressive).")]
    [Range(1, 5)]
    public int aggression;
    [Tooltip("How likely the fish is to inspect/commit to the lure (1 = shy, 5 = curious).")]
    [Range(1, 5)]
    public int curiosity;

    [Header("Modifiers")]
    [Tooltip("Per-species modifiers applied to the Spinning Rod minigame.")]
    public SpinningModifiers spinning;
    [Tooltip("Per-species modifiers applied to the Casting Rod minigame.")]
    public CastingModifiers casting;

    [Header("Size Scaling")]
    [Tooltip("Curves that further scale minigame difficulty based on the specific fish's size within this species.")]
    public SizeScaling sizeScaling = SizeScaling.Default();
}

#region Structs
[Serializable]
public struct SpinningModifiers {
    [Header("Timer")]
    [Tooltip("Multiplies the base fight timer seconds from SpinningRodSettings.\n> 1 = longer fight (harder), < 1 = shorter fight (easier).")]
    [Range(1f, 2f)]
    public float timeMultiplier;

    [Header("Arc")]
    [Tooltip("Multiplies the target arc fill amount (0..1) from SpinningRodSettings.\nSmaller arcs are harder, so < 1 = harder.")]
    [Range(0.5f, 1f)]
    public float arcMultiplier;

    [Header("Relocations")]
    [Tooltip("Adds to the base relocation count from SpinningRodSettings difficulty tier.\nPositive = more relocations (harder), negative = fewer (easier).")]
    public int relocationBonus;

    [Header("Land")]
    [Tooltip("Multiplies land gain per second.\n< 1 = harder to land, > 1 = easier to land.")]
    [Range(0.75f, 1f)]
    public float landGainMultiplier;

    [Header("Slack")]
    [Tooltip("Multiplies how quickly slack fills per second (base + distance-based fill).\n> 1 = slack fills faster (harder).")]
    [Range(0.5f, 2f)] public float slackFillMultiplier;
    [Tooltip("Multiplies how quickly slack decays/clears per second.\n> 1 = slack clears faster (easier).")]
    [Range(0.5f, 2f)] public float slackDecayMultiplier;

    [Header("Steering")]
    [Tooltip("Multiplies steering responsiveness for the cursor (degreesPerPixel and maxCursorDegPerSecond).\n> 1 = more responsive (easier), < 1 = sluggish (harder).")]
    [Range(0.5f, 2f)]
    public float steeringMultiplier;
}

[Serializable]
public struct CastingModifiers {
    [Header("Timer")]
    [Tooltip("Multiplies the base fight timer seconds from CastingRodSettings.\n> 1 = longer fight (harder), < 1 = shorter fight (easier).")]
    [Range(1f, 2f)]
    public float timeMultiplier;

    [Header("Heat")]
    [Tooltip("Multiplies the starting heat (0..1).\n> 1 = starts hotter (harder), < 1 = starts cooler (easier).")]
    [Range(0.5f, 1.5f)]
    public float heatStartMultiplier;
    [Tooltip("Multiplies the backlash threshold (0..1).\nLower thresholds are harder (you backlash sooner), so < 1 = harder, > 1 = easier.")]
    [Range(0.5f, 1.5f)]
    public float backlashThresholdMultiplier;
    [Tooltip("Multiplies heat rise rates when NOT holding (calm + surge).\n> 1 = heats up faster (harder).")]
    [Range(0.5f, 2f)]
    public float heatRiseMultiplier;
    [Tooltip("Multiplies heat fall rates when holding.\n> 1 = cools faster (easier).")]
    [Range(0.5f, 2f)]
    public float heatFallHoldMultiplier;
    [Tooltip("Multiplies the special heatFallHoldSurgeMultiplier used during surge holds.\n> 1 = cools faster during surge holds (easier).")]
    [Range(0.5f, 2f)]
    public float heatFallHoldSurgeMultiplier;

    [Header("Land Rates")]
    [Tooltip("Multiplies land gain per second while NOT holding during calm moments.")]
    [Range(0.5f, 2f)]
    public float landGainNoHoldCalmMultiplier;
    [Tooltip("Multiplies land gain per second while NOT holding during surge moments.")]
    [Range(0.5f, 2f)]
    public float landGainNoHoldSurgeMultiplier;
    [Tooltip("Multiplies land loss per second while holding during surges.\n> 1 = lose progress faster (harder).")]
    [Range(0.5f, 2f)]
    public float landLossHoldSurgeMultiplier;

    [Header("Surge Pulses")]
    [Tooltip("Adds to the base surge pulse count for the selected difficulty tier.\nPositive = more surges (harder), negative = fewer (easier).")]
    public int pulseBonus;

    [Header("Surge Timing")]
    [Tooltip("Multiplies surgeDurationSeconds.\nLonger surges are usually harder because you must manage hold decisions longer.")]
    [Range(0.5f, 2f)]
    public float surgeDurationMultiplier;
    [Tooltip("Multiplies pulseJitterSeconds.\nMore jitter makes timing less predictable (harder).")]
    [Range(0.5f, 2f)]
    public float pulseJitterMultiplier;

    [Tooltip("Multiplies pulseStartEndMarginSeconds.\nHigher margins reduce how early/late surges can occur (often slightly easier).")]
    [Range(0.5f, 2f)]
    public float pulseStartEndMarginMultiplier;
}

[Serializable]
public struct SizeScaling {
    [Header("Spinning Scaling by Size")]
    [Tooltip("Multiplier applied to the final spinning timer seconds based on fish size (0..1 normalized size).")]
    public AnimationCurve timeBySize;
    [Tooltip("Multiplier applied to the final spinning target arc fill amount based on fish size.")]
    public AnimationCurve arcBySize;
    [Tooltip("Multiplier applied to the final spinning slack fill rates based on fish size.")]
    public AnimationCurve slackBySize;
    [Tooltip("Multiplier applied to the final spinning land gain rate based on fish size.")]
    public AnimationCurve landBySize;

    [Header("Casting Scaling by Size")]
    [Tooltip("Multiplier applied to casting heat rates based on fish size (bigger fish typically generate/retain more heat pressure).")]
    public AnimationCurve heatBySize;
    [Tooltip("Adds surge pulses based on fish size (value is rounded to int at runtime).")]
    public AnimationCurve pulseBySize;

    public static SizeScaling Default() {
        SizeScaling scaling = new SizeScaling();

        static AnimationCurve LateRamp(float endValue, float midX = 0.7f, float midValue = 1.02f) {

            midX = Mathf.Clamp01(midX);

            // For decreasing curves (endValue < 1), keep midValue slightly below 1.
            if (endValue < 1f) midValue = Mathf.Min(midValue, 0.98f);

            var curve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(midX, midValue),
                new Keyframe(1f, endValue)
            );

            // Smooth tangents so the change isn't abrupt
            for (int i = 0; i < curve.length; i++) {
                curve.SmoothTangents(i, 0.6f);
            }
            return curve;
        }

        // Spinning
        scaling.timeBySize = LateRamp(endValue: 1.15f, midX: 0.7f, midValue: 1.02f);
        scaling.arcBySize = LateRamp(endValue: 0.90f, midX: 0.7f, midValue: 0.98f);
        scaling.slackBySize = LateRamp(endValue: 1.25f, midX: 0.7f, midValue: 1.05f);
        scaling.landBySize = LateRamp(endValue: 0.90f, midX: 0.7f, midValue: 0.98f);

        // Casting
        scaling.heatBySize = LateRamp(endValue: 1.20f, midX: 0.7f, midValue: 1.03f);

        scaling.pulseBySize = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.7f, 0f),
            new Keyframe(1f, 1f)
        );

        for (int i = 0; i < scaling.pulseBySize.length; i++) {
            scaling.pulseBySize.SmoothTangents(i, 0.6f);
        }

        return scaling;
    }
}
#endregion

#region Enums
public enum Environment {
    Pond = 0,
    River = 1,
    Lake = 2,
    Coastal = 3,
    Ocean = 4
}
#endregion