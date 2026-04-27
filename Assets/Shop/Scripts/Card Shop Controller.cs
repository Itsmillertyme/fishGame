
using UnityEngine;

public class CardShopController : MonoBehaviour {
    #region Variables
    CardShopUI cardShopUI;
    #endregion

    #region Unity Methods
    private void Start() {
        cardShopUI = FindFirstObjectByType<CardShopUI>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            cardShopUI.OpenShop();
        }
    }
    #endregion

}
