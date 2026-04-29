using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TackleBoxUI : MonoBehaviour {
    [SerializeField] private GameObject tackleBoxPanel;
    [SerializeField] private TackleBox tackleBox;
    [SerializeField] private PlayerDataRuntime playerDataRuntime;
    [SerializeField] private CardRegistry cardRegistry;
    [SerializeField] private GearMapper gearMapper;

    [Header("Slots")]
    [SerializeField] private Image rodSlot;
    [SerializeField] private Image reelSlot;
    [SerializeField] private Image lureSlot;

    [Header("List Container")]
    [SerializeField] private Transform cardListContainer;
    [SerializeField] private GameObject cardSlotPrefab;

    private void Start() {

        foreach (int cardId in playerDataRuntime.Data.cardIdsOwned) {
            EquipCard(cardRegistry.GetById(cardId));
        }

        tackleBoxPanel.SetActive(false);
    }

#if UNITY_EDITOR


    private void Update() {
        // Check if Q key was just pressed
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) {
            ToggleTackleBox();
        }
    }
#endif
    private void ToggleTackleBox() {
        if (tackleBoxPanel.activeSelf) {
            Close();
        }
        else {
            Open();
        }
    }


    public void Open() {
        tackleBoxPanel.SetActive(true);
        RefreshUI();
    }

    public void Close() {
        tackleBoxPanel.SetActive(false);
    }

    private void RefreshUI() {
        rodSlot.sprite = GetSpriteForEquipped(tackleBox.GetEquippedRod(), UpgradeType.Rod);
        reelSlot.sprite = GetSpriteForEquipped(tackleBox.GetEquippedReel(), UpgradeType.Reel);
        lureSlot.sprite = GetSpriteForEquipped(tackleBox.GetEquippedLure(), UpgradeType.Lure);

        foreach (Transform child in cardListContainer) Destroy(child.gameObject);

        foreach (int cardId in playerDataRuntime.Data.cardIdsOwned) {
            Card card = cardRegistry.GetById(cardId);
            if (card == null) continue;

            GameObject slot = Instantiate(cardSlotPrefab, cardListContainer);

            Image slotImage = slot.GetComponent<Image>();
            slotImage.sprite = card.cardImage;

            slot.GetComponent<Button>().onClick.AddListener(() => EquipCard(card));
        }
    }

    private Sprite GetSpriteForEquipped(Object equippedItem, UpgradeType type) {
        if (equippedItem == null) return null;

        // Find the Card ID that matches this gear item to get its sprite
        foreach (var mapping in gearMapper.mappings) {
            if (mapping.gearItem == equippedItem) {
                Card card = cardRegistry.GetById(mapping.cardId);
                return card != null ? card.cardImage : null;
            }
        }
        return null;
    }

    public void EquipCard(Card card) {
        tackleBox.EquipCard(card);
        playerDataRuntime.EquipCard(card.id);
        RefreshUI();
    }
}