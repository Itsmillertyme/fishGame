using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Tackle/Rod", fileName = "RodItem")]
public class RodItem : ScriptableObject {
    public string displayName;
    public Sprite icon;
    public GameObject rodPrefab;

    [Header("Modifiers")]
    [Tooltip("Multiplies land gain during the minigame.")]
    [Range(0.5f, 2f)] public float landGainMultiplier = 1f;

    [Tooltip("Casting only: multiplies land loss during surge holds. < 1 = less punishing.")]
    [Range(0.5f, 2f)] public float castingLandLossMultiplier = 1f;
}