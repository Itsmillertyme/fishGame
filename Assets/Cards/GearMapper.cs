using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Gear Mapper")]
public class GearMapper : ScriptableObject {
    [System.Serializable]
    public struct CardItemMap {
        public int cardId;
        public ScriptableObject gearItem;
    }

    public List<CardItemMap> mappings;
    private Dictionary<int, ScriptableObject> _map;

    private void OnEnable() {
        _map = new Dictionary<int, ScriptableObject>();
        foreach (var item in mappings) {
            if (!_map.ContainsKey(item.cardId)) _map[item.cardId] = item.gearItem;
        }
    }

    public T GetItemForCard<T>(int cardId) where T : ScriptableObject {
        if (_map == null) OnEnable();
        return _map.TryGetValue(cardId, out var item) ? item as T : null;
    }

    public int GetCardforItem(ScriptableObject item) {
        if (_map == null) OnEnable();
        foreach (var kvp in _map) {
            if (kvp.Value == item) {
                return kvp.Key;
            }
        }

        return -1;
    }
}