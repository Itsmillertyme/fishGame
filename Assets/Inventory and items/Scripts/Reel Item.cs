using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Tackle/Reel", fileName = "ReelItem")]
public class ReelItem : ScriptableObject {
    public string displayName;
    public Sprite icon;
    public GameObject reelPrefab;

    public ReelType reelType;

    [Header("Spinning Modifiers")]
    [Range(0.1f, 5f)] public float slackGainMultiplier = 1f;   // > 1 = harder
    [Range(0.1f, 5f)] public float slackDecayMultiplier = 1f;  // > 1 = easier

    [Header("Casting Modifiers")]
    [Range(0.1f, 5f)] public float heatGainMultiplier = 1f;    // > 1 = harder
    [Range(0.1f, 5f)] public float heatDecayMultiplier = 1f;   // > 1 = easier


}
public enum ReelType {
    Spinning = 0,
    Casting = 1
}