using UnityEngine;

public class MobileUIController : MonoBehaviour {
    [Header("References")]
    [SerializeField] private GameObject mobileControlsRoot;
    [SerializeField] private MobileInputBridge inputBridge;

    [Header("Settings")]
    [Tooltip("Force mobile controls in editor for testing")]
    [SerializeField] private bool forceMobileInEditor = true;

    private void Start() {
        bool isMobile = Application.isMobilePlatform;

#if UNITY_EDITOR
        if (forceMobileInEditor) {
            isMobile = true;
        }
#endif

        SetMobileMode(isMobile);
    }

    public void SetMobileMode(bool enabled) {
        if (mobileControlsRoot != null) {
            mobileControlsRoot.SetActive(enabled);
        }

        if (inputBridge != null) {
            inputBridge.MobileInputEnabled = enabled;
        }

        if (enabled) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
    }
}