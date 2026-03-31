using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
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
            Debug.LogWarning("[MobileJumpButton] No MobileInputBridge found.");
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        mobileInputBridge?.OnJumpPressed();
    }

    public void OnPointerUp(PointerEventData eventData) {
        mobileInputBridge?.OnJumpReleased();
    }
    #endregion
}