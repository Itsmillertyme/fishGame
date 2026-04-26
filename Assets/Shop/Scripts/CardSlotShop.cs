using System;
using UnityEngine;
using UnityEngine.UI;

public class CardShopSlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image cardImage;

    private Card currentCard;
    public Action<Card> onClicked;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    public void SetCard(Card card)
    {
        currentCard = card;

        if (card == null)
        {
            cardImage.enabled = false;
            button.interactable = false;
            return;
        }

        cardImage.enabled = true;
        cardImage.sprite = card.cardImage;
        button.interactable = true;
    }

    private void HandleClick()
    {
        onClicked?.Invoke(currentCard);
    }
}