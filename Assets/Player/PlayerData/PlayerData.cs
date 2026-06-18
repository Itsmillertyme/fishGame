using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData {
    public string playerId;

    public bool hasSeenCastingMinigameTutorial;

    public int money;

    public List<string> fishDiscoveredIds = new List<string>();
    public List<FishCatchEntry> fishCaughtPerSpecies = new List<FishCatchEntry>();

    public List<int> cardIdsOwned = new List<int>();
    public List<string> cosmeticIdsOwned = new List<string>();
    public List<string> areaIdsDiscovered = new List<string>();

    public string equippedPlayerHatCosmeticId;
    public string equippedPlayerShirtCosmeticId;
    public string equippedPlayerBootsCosmeticId;

    public int equippedRodCardId;
    public int equippedReelCardId;
    public int equippedLureCardId;

    public List<ChallengeSaveEntry> challengeStates = new List<ChallengeSaveEntry>();
}

[Serializable]
public class FishCatchEntry {
    public string speciesId;
    public int totalCaught;
}

[Serializable]
public class ChallengeSaveEntry
{
    public string waterBodyId;
    public string challengeId;
    public int progress;
    public bool isClaimed;
}
