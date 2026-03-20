using UnityEngine;

public enum CosmeticType
{
    PlayerHat,
    PlayerShirt,
    PlayerBoots
}

[CreateAssetMenu(fileName = "Cosmetics",
    menuName = "cosmetic/Cosmetics")]
public class CosmeticDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    [Header("Visuals")]
    [SerializeField] private GameObject prefab;

    [Header("Logic")]
    [SerializeField] private CosmeticType type;

    public string Id => id;
    public string DisplayName => displayName;
    public GameObject Prefab => prefab;
    public CosmeticType Type => type;
}
