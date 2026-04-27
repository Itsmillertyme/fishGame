using TMPro;
using UnityEngine;

public class MoneyUIController : MonoBehaviour {

    #region Variables
    [SerializeField] TextMeshProUGUI moneyTMP;

    PlayerDataRuntime player;
    #endregion

    #region Unity Methods

    private void Awake() {
        player = FindFirstObjectByType<PlayerDataRuntime>();
    }
    private void OnEnable() {
        player.OnMoneyAmountChanged += UpdateMoneyGUI;
    }
    private void OnDisable() {
        player.OnMoneyAmountChanged -= UpdateMoneyGUI;
    }

    #endregion

    #region Utility Methods
    void UpdateMoneyGUI(int totalAmount) {
        moneyTMP.text = $"${totalAmount}";
    }
    #endregion

}
