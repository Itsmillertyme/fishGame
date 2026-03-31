using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Fishing/Fish Spawn Table", fileName = "FishSpawnTable")]
public class FishSpawnTable : ScriptableObject {
    #region Varbiables
    public FishSpawnEntry[] entries;
    #endregion

    #region Utility Methods
    public FishRoll Roll() {
        if (entries == null || entries.Length == 0) {
            return default;
        }

        FishSpawnEntry entry = PickWeightedEntry(entries);

        bool isTrophy = entry.trophyConfig != null && UnityEngine.Random.value < Mathf.Clamp01(entry.trophyChance);

        FishSpeciesConfig config = isTrophy ? entry.trophyConfig : entry.normalConfig;

        float size01 = RollSize01(entry.sizeSkewPower);

        return new FishRoll {
            config = config,
            size01 = size01,
            isTrophy = isTrophy
        };
    }

    static FishSpawnEntry PickWeightedEntry(FishSpawnEntry[] list) {
        float total = 0f;
        for (int i = 0; i < list.Length; i++) {
            total += Mathf.Max(0f, list[i].weight);
        }

        if (total <= 0f) return list[0];

        float rand = UnityEngine.Random.value * total;
        float running = 0f;

        for (int i = 0; i < list.Length; i++) {
            running += Mathf.Max(0f, list[i].weight);
            if (rand <= running) return list[i];
        }

        return list[list.Length - 1];
    }

    static float RollSize01(float skewPower) {
        // skewPower > 1 biases smaller fish, skewPower < 1 biases larger fish
        float pow = Mathf.Max(0.01f, skewPower);
        return Mathf.Clamp01(Mathf.Pow(UnityEngine.Random.value, pow));
    }
    #endregion
}

#region Structs

[Serializable]
public struct FishSpawnEntry {
    [Header("Species Assets")]
    public FishSpeciesConfig normalConfig;
    public FishSpeciesConfig trophyConfig;

    [Header("Spawn")]
    [Min(0f)] public float weight;

    [Header("Trophy")]
    [Range(0f, 1f)] public float trophyChance;

    [Header("Size Distribution")]
    [Tooltip("Bigger number = more small fish")]
    [Range(0.25f, 6f)] public float sizeSkewPower;
}

public struct FishRoll {
    public FishSpeciesConfig config;
    public float size01;
    public bool isTrophy;
}
#endregion