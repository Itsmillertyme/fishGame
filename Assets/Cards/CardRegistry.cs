using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardRegistry", menuName = "Card/Card Registry")]
public class CardRegistry : ScriptableObject
{
    [SerializeField] private List<Card> cards = new List<Card>();
    private Dictionary<int, Card> _byId;

    void OnEnable()
    {
        _byId = new Dictionary<int, Card>();
        foreach (var c in cards)
        {
            if (c != null && c.id != 0)
                _byId[c.id] = c;
        }
    }

    public Card GetById(int id)
    {
        if (id == 0 || _byId == null) return null;
        return _byId.TryGetValue(id, out var card) ? card : null;
    }
}
