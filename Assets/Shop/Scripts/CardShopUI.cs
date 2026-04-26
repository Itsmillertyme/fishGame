using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardShopUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CardRegistry cardRegistry;
    [SerializeField] private TackleBox tackleBox;

    [Header("Shop UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button shuffleButton;

    [Header("Card Slots")]
    [SerializeField] private CardShopSlot slot1;
    [SerializeField] private CardShopSlot slot2;
    [SerializeField] private CardShopSlot slot3;

    [Header("Detail Panel")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailCardImage;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Button backButton;

    [Header("Save Data")]
    [SerializeField] private PlayerDataRuntime playerDataRuntime;
    [SerializeField] private int cardPrice = 100;

    private Card currentSelectedCard;
    private List<Card> currentShownCards = new List<Card>();

    private void Awake()
    {
        closeButton.onClick.AddListener(CloseShop);
        shuffleButton.onClick.AddListener(ShuffleCards);

        purchaseButton.onClick.AddListener(PurchaseSelectedCard);
        backButton.onClick.AddListener(CloseDetailPanel);

        slot1.onClicked = SelectCard;
        slot2.onClicked = SelectCard;
        slot3.onClicked = SelectCard;
    }

    private void OnEnable()
    {
        OpenShop();
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        detailPanel.SetActive(false);
        RefreshCards();
    }

    public void CloseShop()
    {
        detailPanel.SetActive(false);
        shopPanel.SetActive(false);
    }

    public void ShuffleCards()
    {
        RefreshCards();
    }

    private void RefreshCards()
    {
        var allCards = GetAllValidCards();
        currentShownCards = allCards.OrderBy(x => Random.value).Take(3).ToList();

        slot1.SetCard(currentShownCards.Count > 0 ? currentShownCards[0] : null);
        slot2.SetCard(currentShownCards.Count > 1 ? currentShownCards[1] : null);
        slot3.SetCard(currentShownCards.Count > 2 ? currentShownCards[2] : null);
    }

    private List<Card> GetAllValidCards()
    {
        return cardRegistry == null ? new List<Card>() : cardRegistry.GetAllCards();
    }

    private void SelectCard(Card card)
    {
        if (card == null) return;

        currentSelectedCard = card;
        detailPanel.SetActive(true);

        detailCardImage.sprite = card.cardImage;
    }

    private void CloseDetailPanel()
    {
        currentSelectedCard = null;
        detailPanel.SetActive(false);
    }

    private void PurchaseSelectedCard()
    {
        if (currentSelectedCard == null) return;

        ApplyCardToTackleBox(currentSelectedCard);
        CloseShop();
    }

    private void ApplyCardToTackleBox(Card card)
    {
        if (tackleBox == null || card == null) return;

        tackleBox.EquipCard(card);

        playerDataRuntime.EquipCard(card.id);
    }
}