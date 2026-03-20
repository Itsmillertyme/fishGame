using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CosmeticRegistry",
    menuName = "cosmetic/Cosmetic Registry")]
public class CosmeticRegistry : ScriptableObject
{
    [SerializeField]
    private List<CosmeticDefinition> cosmetics = new List<CosmeticDefinition>();

    private Dictionary<string, CosmeticDefinition> lookup;

    void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, CosmeticDefinition>();
        foreach (var cosmetic in cosmetics)
        {
            if (cosmetic == null || string.IsNullOrEmpty(cosmetic.Id))
                continue;

            if (!lookup.ContainsKey(cosmetic.Id))
                lookup.Add(cosmetic.Id, cosmetic);
            else
                Debug.LogWarning($"Duplicate cosmetic id: {cosmetic.Id}", cosmetic);
        }
    }

    public CosmeticDefinition GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        if (lookup == null || lookup.Count == 0)
            BuildLookup();

        lookup.TryGetValue(id, out var result);
        return result;
    }

    public IReadOnlyList<CosmeticDefinition> AllCosmetics => cosmetics;
}
