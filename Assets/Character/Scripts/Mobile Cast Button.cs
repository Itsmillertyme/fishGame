using UnityEngine;
using UnityEngine.EventSystems;

public class MobileCastButton : MonoBehaviour, IPointerClickHandler {
    #region Variables
    private MobileInputBridge mobileInputBridge;
    #endregion

    #region Unity Methods
    private void Awake() {
        Rebind();
    }

    private void OnEnable() {
        Rebind();
    }
    #endregion

    #region Utility Methods
    private void Rebind() {
        mobileInputBridge = FindFirstObjectByType<MobileInputBridge>();

        if (mobileInputBridge == null) {
            Debug.LogWarning("[MobileCastButton] No MobileInputBridge found.");
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        mobileInputBridge?.OnCastPressed();
    }
    #endregion
}