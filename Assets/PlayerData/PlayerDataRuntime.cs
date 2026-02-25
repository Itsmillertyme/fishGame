using System.Collections.Generic;
using UnityEngine;

public class PlayerDataRuntime : MonoBehaviour
{
    public PlayerData Data { get; private set; } = new PlayerData();

    private Dictionary<string, FishCatchEntry> _fishCatchById;
    private HashSet<string> _fishDiscovered;
    private HashSet<string> _cardsOwned;
    private HashSet<string> _cosmeticsOwned;
    private HashSet<string> _areasDiscovered;

    public void InitializeFromLoaded(PlayerData loaded)
    {
        Data = loaded ?? new PlayerData();
        RebuildLookups();
    }

    private void RebuildLookups()
    {
        _fishCatchById = new Dictionary<string, FishCatchEntry>();
        foreach (var entry in Data.fishCaughtPerSpecies)
        {
            if (entry != null && !string.IsNullOrEmpty(entry.speciesId))
                _fishCatchById[entry.speciesId] = entry;
        }

        _fishDiscovered = new HashSet<string>(Data.fishDiscoveredIds ?? new List<string>());
        _cardsOwned = new HashSet<string>(Data.cardIdsOwned ?? new List<string>());
        _cosmeticsOwned = new HashSet<string>(Data.cosmeticIdsOwned ?? new List<string>());
        _areasDiscovered = new HashSet<string>(Data.areaIdsDiscovered ?? new List<string>());
    }

    // ---- Public API ----

    public void OnFishCaught(string speciesId)
    {
        if (string.IsNullOrEmpty(speciesId)) return;

        // Discovered
        if (_fishDiscovered.Add(speciesId))
            Data.fishDiscoveredIds.Add(speciesId);

        // Caught count
        if (!_fishCatchById.TryGetValue(speciesId, out var entry))
        {
            entry = new FishCatchEntry { speciesId = speciesId, totalCaught = 0 };
            _fishCatchById[speciesId] = entry;
            Data.fishCaughtPerSpecies.Add(entry);
        }
        entry.totalCaught++;
    }

    public int GetTotalCaught(string speciesId)
    {
        return _fishCatchById != null && _fishCatchById.TryGetValue(speciesId, out var entry)
            ? entry.totalCaught
            : 0;
    }

    public bool IsFishDiscovered(string speciesId) => _fishDiscovered.Contains(speciesId);

    public void AddCard(string cardId)
    {
        if (_cardsOwned.Add(cardId))
            Data.cardIdsOwned.Add(cardId);
    }

    public bool HasCard(string cardId) => _cardsOwned.Contains(cardId);

    public void AddCosmetic(string cosmeticId)
    {
        if (_cosmeticsOwned.Add(cosmeticId))
            Data.cosmeticIdsOwned.Add(cosmeticId);
    }

    public bool HasCosmetic(string cosmeticId) => _cosmeticsOwned.Contains(cosmeticId);

    public void DiscoverArea(string areaId)
    {
        if (_areasDiscovered.Add(areaId))
            Data.areaIdsDiscovered.Add(areaId);
    }

    public bool IsAreaDiscovered(string areaId) => _areasDiscovered.Contains(areaId);
}
