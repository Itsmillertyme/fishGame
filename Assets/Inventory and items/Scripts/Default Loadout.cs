// DefaultTackleLoadoutSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Tackle/Loadout/Default Loadout", fileName = "DefaultLoadout")]
public class DefaultLoadout : ScriptableObject {

    [Header("Starting Gear")]
    public RodItem startingRod;
    public ReelItem startingReel;
    public LureItem startingLure;
}