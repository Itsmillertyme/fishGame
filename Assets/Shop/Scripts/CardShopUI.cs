using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardShopUI : MonoBehaviour {
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

    [Header("SFX")]
    [SerializeField] AudioClip purchaseSFX;
    [SerializeField] AudioClip deniedSFX;
    [SerializeField] AudioClip shuffleSFX;

    [Header("Save Data")]
    [SerializeField] private PlayerDataRuntime playerDataRuntime;
    [SerializeField] private int cardPrice = 100;

    private Card currentSelectedCard;
    private List<Card> currentShownCards = new List<Card>();

    public static event Action<int> OnShopEntered;
    public static event Action<int> OnShopExited;

    private void Awake() {
        closeButton.onClick.AddListener(CloseShop);
        shuffleButton.onClick.AddListener(ShuffleCards);

        purchaseButton.onClick.AddListener(PurchaseSelectedCard);
        backButton.onClick.AddListener(CloseDetailPanel);

        slot1.onClicked = SelectCard;
        slot2.onClicked = SelectCard;
        slot3.onClicked = SelectCard;
    }

    private void OnEnable() {
        RefreshCards();
        CloseShop();
    }

    public void OpenShop() {
        OnShopEntered?.Invoke(55);

        shopPanel.SetActive(true);
        detailPanel.SetActive(false);
        RefreshCards();

        //Cursor.visible = true;
        //Cursor.lockState = CursorLockMode.None;
    }

    public void CloseShop() {
        detailPanel.SetActive(false);
        shopPanel.SetActive(false);

        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;

        OnShopExited?.Invoke(55);
    }

    public void ShuffleCards() {
        if (playerDataRuntime.GetMoney() >= 10) {
            playerDataRuntime.AddMoney(-10);
            RefreshCards();

            AudioSource.PlayClipAtPoint(shuffleSFX, playerDataRuntime.transform.position);
        }
        else {
            AudioSource.PlayClipAtPoint(deniedSFX, playerDataRuntime.transform.position);
        }

    }

    private void RefreshCards() {
        var defaultCardIds = GetDefaultCardIds();

        var allCards = GetAllValidCards().Where(card => card != null && !defaultCardIds.Contains(card.id)).ToList();

        currentShownCards = allCards.OrderBy(card => UnityEngine.Random.value).Take(3).ToList();

        slot1.SetCard(currentShownCards.Count > 0 ? currentShownCards[0] : null);
        slot2.SetCard(currentShownCards.Count > 1 ? currentShownCards[1] : null);
        slot3.SetCard(currentShownCards.Count > 2 ? currentShownCards[2] : null);
    }

    private List<Card> GetAllValidCards() {
        return cardRegistry == null ? new List<Card>() : cardRegistry.GetAllCards();
    }

    private void SelectCard(Card card) {
        if (card == null) return;

        currentSelectedCard = card;
        detailPanel.SetActive(true);

        detailCardImage.sprite = card.cardImage;
    }

    private void CloseDetailPanel() {
        currentSelectedCard = null;
        detailPanel.SetActive(false);
    }

    private void PurchaseSelectedCard() {
        if (currentSelectedCard == null) return;

        if (playerDataRuntime.GetMoney() >= cardPrice) {
            playerDataRuntime.AddMoney(-cardPrice);
            AudioSource.PlayClipAtPoint(purchaseSFX, playerDataRuntime.transform.position);
            ApplyCardToTackleBox(currentSelectedCard);
            CloseShop();
        }
        else {
            AudioSource.PlayClipAtPoint(deniedSFX, playerDataRuntime.transform.position);
        }
    }

    private void ApplyCardToTackleBox(Card card) {
        if (tackleBox == null || card == null) return;

        tackleBox.EquipCard(card);

        playerDataRuntime.AddCard(card.id);
        playerDataRuntime.EquipCard(card.id);
    }

    private HashSet<int> GetDefaultCardIds() {
        HashSet<int> defaultCardIds = new HashSet<int>();

        TackleBox playerTackleBox = FindFirstObjectByType<TackleBox>();

        if (playerTackleBox == null) {
            return defaultCardIds;
        }

        AddDefaultCardId(defaultCardIds, playerTackleBox.GearMapper, playerTackleBox.DefaultLoadout.startingRod);
        AddDefaultCardId(defaultCardIds, playerTackleBox.GearMapper, playerTackleBox.DefaultLoadout.startingReel);
        AddDefaultCardId(defaultCardIds, playerTackleBox.GearMapper, playerTackleBox.DefaultLoadout.startingLure);

        return defaultCardIds;
    }

    private void AddDefaultCardId(HashSet<int> defaultCardIds, GearMapper gearMapper, ScriptableObject item) {
        if (item == null) {
            return;
        }

        int cardId = cardRegistry.GetById(gearMapper.GetCardforItem(item)).id;

        if (cardId >= 0) {
            defaultCardIds.Add(cardId);
        }
    }

}