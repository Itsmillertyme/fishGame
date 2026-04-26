using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Gear Mapper")]
public class GearMapper : ScriptableObject
{
    [System.Serializable]
    public struct CardItemMap
    {
        public int cardId;
        public ScriptableObject gearItem;
    }

    public List<CardItemMap> mappings;

    public ScriptableObject GetItemForCard(int cardId)
    {
        var match = mappings.Find(x => x.cardId == cardId);
        return match.gearItem;
    }
}