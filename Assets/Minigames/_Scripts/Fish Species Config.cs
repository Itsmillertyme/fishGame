using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Fish Species Config", fileName = "FishSpeciesConfig")]
public class FishSpeciesConfig : ScriptableObject {
    [Header("Identity")]
    public string displayName;
    public Sprite icon;

    [Header("Modifiers")]
    public SpinningModifiers spinning;
    public CastingModifiers casting;

    [Header("Size Scaling")]
    public SizeScaling sizeScaling = SizeScaling.Default();
}

#region Structs
[Serializable]
public struct SpinningModifiers {
    [Header("Timer")]
    [Range(1f, 2f)]
    public float timeMultiplier;

    [Header("Arc")]
    [Range(0.5f, 1f)]
    public float arcMultiplier;

    [Header("Relocations")]
    public int relocationBonus;

    [Header("Land")]
    [Range(0.75f, 1f)]
    public float landGainMultiplier;        // <1 -> harder to land

    [Header("Slack")]
    [Range(0.5f, 2f)] public float slackFillMultiplier;
    [Range(0.5f, 2f)] public float slackDecayMultiplier;// >1 -> Slack clears faster

    [Header("Steering")]
    [Range(0.5f, 2f)]
    public float steeringMultiplier;      // multiplies degreesPerPixel + maxCursorDegPerSecond
}

[Serializable]
public struct CastingModifiers {

}

[Serializable]
public struct SizeScaling {
    [Header("Spinning Scaling by Size")]
    public AnimationCurve timeBySize;       // multiply time
    public AnimationCurve arcBySize;        // multiply arc fill
    public AnimationCurve slackBySize;      // multiply slack fill
    public AnimationCurve landBySize;       // multiply land gain

    [Header("Casting Scaling by Size")]
    public AnimationCurve heatBySize;       // multiply heat rates
    public AnimationCurve pulseBySize;      // add pulses (rounded)

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
        scaling.timeBySize = LateRamp(endValue: 1.15f, midX: 0.7f, midValue: 1.02f);
        scaling.arcBySize = LateRamp(endValue: 0.90f, midX: 0.7f, midValue: 0.98f);
        scaling.slackBySize = LateRamp(endValue: 1.25f, midX: 0.7f, midValue: 1.05f);
        scaling.landBySize = LateRamp(endValue: 0.90f, midX: 0.7f, midValue: 0.98f);

        return scaling;
    }
}
#endregion
