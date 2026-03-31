using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Anglerpedia", fileName = "Anglerpedia")]
public class Anglerpedia : ScriptableObject
{
    [Tooltip("All fish species ScriptableObjects included in the game.")]
    public List<FishSpeciesConfig> allSpecies = new List<FishSpeciesConfig>();

    private Dictionary<string, FishSpeciesConfig> _byId;
    private Dictionary<Environment, List<FishSpeciesConfig>> _byEnvironment;
    private List<FishSpeciesConfig> _trophySpecies;

    public void InitializeAnglerpedia()
    {
        if (_byId != null) return;

        _byId = new Dictionary<string, FishSpeciesConfig>(StringComparer.OrdinalIgnoreCase);
        _byEnvironment = new Dictionary<Environment, List<FishSpeciesConfig>>();
        _trophySpecies = new List<FishSpeciesConfig>();

        foreach (Environment env in Enum.GetValues(typeof(Environment)))
        {
            _byEnvironment[env] = new List<FishSpeciesConfig>();
        }

        foreach (var species in allSpecies)
        {
            if (species == null)
                continue;

            foreach (var env in species.environments)
            {
                if (_byEnvironment.TryGetValue(env, out var list))
                    list.Add(species);
            }

            if (species.isTrophy)
                _trophySpecies.Add(species);
        }
    }

    public IReadOnlyList<FishSpeciesConfig> GetAll()
    {
        return allSpecies;
    }

    public IReadOnlyList<FishSpeciesConfig> GetByEnvironment(Environment env)
    {
        InitializeAnglerpedia();
        return _byEnvironment.TryGetValue(env, out var list) ? list : Array.Empty<FishSpeciesConfig>();
    }

    public IReadOnlyList<FishSpeciesConfig> GetTrophySpecies()
    {
        InitializeAnglerpedia();
        return _trophySpecies;
    }
}

