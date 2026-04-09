using System.Collections.Generic;
using UnityEngine;

public class PlayerDataRuntime : MonoBehaviour {
    public PlayerData Data { get; private set; } = new PlayerData();

    private Dictionary<string, FishCatchEntry> _fishCatchById;
    private HashSet<string> _fishDiscovered;
    private HashSet<int> _cardsOwned;
    private HashSet<string> _cosmeticsOwned;
    private HashSet<string> _areasDiscovered;

    [Header("Refs")]
    [SerializeField] private CosmeticRegistry cosmeticRegistry;
    [SerializeField] private CardRegistry cardRegistry;

    private void Awake() {
        Data ??= new PlayerData();
        RebuildLookups();
    }

    public void InitializeFromLoaded(PlayerData loaded) {
        Data = loaded ?? new PlayerData();
        RebuildLookups();
    }

    private void RebuildLookups() {
        _fishCatchById = new Dictionary<string, FishCatchEntry>();
        foreach (var entry in Data.fishCaughtPerSpecies) {
            if (entry != null && !string.IsNullOrEmpty(entry.speciesId))
                _fishCatchById[entry.speciesId] = entry;
        }

        _fishDiscovered = new HashSet<string>(Data.fishDiscoveredIds ?? new List<string>());
        _cardsOwned = new HashSet<int>(Data.cardIdsOwned ?? new List<int>());
        _cosmeticsOwned = new HashSet<string>(Data.cosmeticIdsOwned ?? new List<string>());
        _areasDiscovered = new HashSet<string>(Data.areaIdsDiscovered ?? new List<string>());
    }

    // ---- Public API ----

    public void OnFishCaught(string speciesId) {
        if (string.IsNullOrEmpty(speciesId)) return;

        // Discovered
        if (_fishDiscovered.Add(speciesId))
            Data.fishDiscoveredIds.Add(speciesId);

        // Caught count
        if (!_fishCatchById.TryGetValue(speciesId, out var entry)) {
            entry = new FishCatchEntry { speciesId = speciesId, totalCaught = 0 };
            _fishCatchById[speciesId] = entry;
            Data.fishCaughtPerSpecies.Add(entry);
        }
        entry.totalCaught++;
    }

    public CosmeticDefinition GetCosmeticDefinition(string cosmeticId) {
        if (cosmeticRegistry == null) {
            Debug.LogError("CosmeticRegistry is not assigned on PlayerDataRuntime.");
            return null;
        }
        return cosmeticRegistry.GetById(cosmeticId);
    }

    // Equip by cosmetic id (checks ownership and type)
    public bool EquipCosmetic(string cosmeticId) {
        var def = GetCosmeticDefinition(cosmeticId);
        if (def == null)
            return false;

        if (!HasCosmetic(cosmeticId)) {
            Debug.LogWarning($"Attempted to equip cosmetic not owned: {cosmeticId}");
            return false;
        }

        switch (def.Type) {
            case CosmeticType.PlayerHat:
                Data.equippedPlayerHatCosmeticId = cosmeticId;
                break;
            case CosmeticType.PlayerShirt:
                Data.equippedPlayerShirtCosmeticId = cosmeticId;
                break;
            case CosmeticType.PlayerBoots:
                Data.equippedPlayerBootsCosmeticId = cosmeticId;
                break;
            default:
                return false;
        }

        // Later: notify avatar visual controller here
        // OnEquippedCosmeticsChanged?.Invoke();

        return true;
    }

    public string GetEquippedCosmeticId(CosmeticType type) {
        return type switch {
            CosmeticType.PlayerHat => Data.equippedPlayerHatCosmeticId,
            CosmeticType.PlayerShirt => Data.equippedPlayerShirtCosmeticId,
            CosmeticType.PlayerBoots => Data.equippedPlayerBootsCosmeticId,
            _ => null
        };
    }

    public CosmeticDefinition GetEquippedCosmetic(CosmeticType type) {
        var id = GetEquippedCosmeticId(type);
        return GetCosmeticDefinition(id);
    }

    // Helpers for UI:
    public IEnumerable<CosmeticDefinition> GetOwnedCosmeticsOfType(CosmeticType type) {
        if (cosmeticRegistry == null)
            yield break;

        foreach (var def in cosmeticRegistry.AllCosmetics) {
            if (def != null && def.Type == type && HasCosmetic(def.Id))
                yield return def;
        }
    }

    public void AddCard(int cardId) {
        if (_cardsOwned.Add(cardId))
            Data.cardIdsOwned.Add(cardId);
    }

    public bool HasCard(int cardId) {
        if (_cardsOwned == null) {
            RebuildLookups();
        }

        return _cardsOwned != null && _cardsOwned.Contains(cardId);
    }

    public bool EquipCard(int cardId) {
        var def = GetCardDefinition(cardId);
        if (def == null)
            return false;

        if (!HasCard(cardId)) {
            Debug.LogWarning($"Attempted to equip card not owned: {cardId}");
            return false;
        }

        switch (def.upgradeType) {
            case UpgradeType.Rod:
                Data.equippedRodCardId = cardId;
                break;
            case UpgradeType.Reel:
                Data.equippedReelCardId = cardId;
                break;
            case UpgradeType.Lure:
                Data.equippedLureCardId = cardId;
                break;
            default:
                return false;
        }
        return true;
    }

    public void LoadEquippedCardsToTackleBox(TackleBox tackleBox) {
        // Rod
        var rodCard = GetEquippedCard(UpgradeType.Rod);
        tackleBox.EquipRod(rodCard != null ? rodCard.upgradePrefab.GetComponent<RodItem>() : null);

        // Reel  
        var reelCard = GetEquippedCard(UpgradeType.Reel);
        tackleBox.EquipReel(reelCard != null ? reelCard.upgradePrefab.GetComponent<ReelItem>() : null);

        // Lure
        var lureCard = GetEquippedCard(UpgradeType.Lure);
        tackleBox.EquipLure(lureCard != null ? lureCard.upgradePrefab.GetComponent<LureItem>() : null);
    }

    public Card GetEquippedCard(UpgradeType type) {
        var id = GetEquippedCardId(type);
        return id == 0 ? null : GetCardDefinition(id);
    }


    public int GetEquippedCardId(UpgradeType type) {
        return type switch {
            UpgradeType.Rod => Data.equippedRodCardId,
            UpgradeType.Reel => Data.equippedReelCardId,
            UpgradeType.Lure => Data.equippedLureCardId,
            _ => 0
        };
    }
    public Card GetCardDefinition(int cardId) {
        if (cardRegistry == null) {
            Debug.LogError("CardRegistry is not assigned on PlayerDataRuntime.");
            return null;
        }
        return cardRegistry.GetById(cardId);
    }

    public int GetTotalCaught(string speciesId) {
        return _fishCatchById != null && _fishCatchById.TryGetValue(speciesId, out var entry)
            ? entry.totalCaught
            : 0;
    }

    public bool IsFishDiscovered(string speciesId) {
        if (_fishDiscovered == null) {
            RebuildLookups();
        }

        return !string.IsNullOrEmpty(speciesId) && _fishDiscovered != null && _fishDiscovered.Contains(speciesId);
    }

    public void AddCosmetic(string cosmeticId) {
        if (string.IsNullOrEmpty(cosmeticId))
            return;

        if (_cosmeticsOwned.Add(cosmeticId)) {
            Data.cosmeticIdsOwned.Add(cosmeticId);
        }
    }

    public bool HasCosmetic(string cosmeticId) {
        if (_cosmeticsOwned == null) {
            RebuildLookups();
        }

        return !string.IsNullOrEmpty(cosmeticId) && _cosmeticsOwned != null && _cosmeticsOwned.Contains(cosmeticId);
    }

    public void DiscoverArea(string areaId) {
        if (_areasDiscovered.Add(areaId))
            Data.areaIdsDiscovered.Add(areaId);
    }

    public bool IsAreaDiscovered(string areaId) {
        if (_areasDiscovered == null) {
            RebuildLookups();
        }

        return !string.IsNullOrEmpty(areaId) && _areasDiscovered != null && _areasDiscovered.Contains(areaId);
    }
}
