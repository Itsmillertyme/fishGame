using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Tackle/Lure", fileName = "LureItem")]
public class LureItem : ScriptableObject {
    public string displayName;
    public Sprite icon;
    public GameObject lurePrefab;

    [Header("Modifiers")]
    [Tooltip("Adds seconds to bite delay. Negative = faster bites.")]
    public float biteDelayAddSeconds = 0.75f;

    [Tooltip("Multiplies bite delay. < 1 = faster bites.")]
    [Range(0.25f, 2f)] public float biteDelayMultiplier = 1f;
}