using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string playerId;

    public List<string> fishDiscoveredIds = new List<string>();
    public List<FishCatchEntry> fishCaughtPerSpecies = new List<FishCatchEntry>();

    public List<string> cardIdsOwned = new List<string>();
    public List<string> cosmeticIdsOwned = new List<string>();
    public List<string> areaIdsDiscovered = new List<string>();
}

[Serializable]
public class FishCatchEntry
{
    public string speciesId;
    public int totalCaught;
}

