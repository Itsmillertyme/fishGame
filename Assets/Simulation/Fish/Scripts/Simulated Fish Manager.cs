using System.Collections.Generic;
using UnityEngine;

public class SimulatedFishManager : MonoBehaviour {

    #region Variables
    public static SimulatedFishManager Instance { get; private set; }

    [Header("World References")]
    [SerializeField] private HabitatVolume habitat;
    [SerializeField] private LayerMask obstacleMask = ~0;

    private readonly List<SimulatedFishAgent> fish = new List<SimulatedFishAgent>();

    public HabitatVolume Habitat => habitat;
    public LayerMask ObstacleMask => obstacleMask;
    #endregion

    #region Unity Methods
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    #endregion

    #region Utility Methods
    public void Register(SimulatedFishAgent agent) {
        if (agent == null) return;
        if (fish.Contains(agent)) return;
        fish.Add(agent);
    }

    public void Unregister(SimulatedFishAgent agent) {
        if (agent == null) return;
        fish.Remove(agent);
    }

    public List<SimulatedFishAgent> GetFishList() {
        return fish;
    }
    #endregion






}
