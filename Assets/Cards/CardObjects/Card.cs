using UnityEngine;

public enum UpgradeType
{
    Rod,
    Reel,
    Lure
}

[CreateAssetMenu(
    fileName = "NewCard",
    menuName = "Fishing/Card",
    order = 0)]
public class Card : ScriptableObject
{
    [Header("Type")]
    public UpgradeType upgradeType;

    [Header("ID")]
    [Tooltip("ID used for saving and loading")]
    public int id;

    [Header("Card Name")]
    [Tooltip("Name of the card")]
    public string cardName;

    [TextArea]
    [Tooltip("Description of the passive effect shown to the player.")]
    public string passiveDescription;

    [Tooltip("Card's statistics")]
    //Flat stats
    public float strength;
    public float luck;
    public float attraction;

    //Specific effects
    //Area based
    public bool oceanBoost;
    public bool pondBoost;
    public bool lakeBoost;
    public bool riverBoost;
    public bool freshwaterBoost;
    public bool saltwaterBoost;

    //Personality based
    public bool timidBoost;
    public bool aggressiveBoost;


    [Header("Visuals")]
    [Tooltip("World / in-game prefab for this upgrade.")]
    public GameObject upgradePrefab;

    [Tooltip("Sprite used as the card artwork in UI.")]
    public Sprite cardImage;
}
