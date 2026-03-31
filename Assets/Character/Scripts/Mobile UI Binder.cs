using System.Collections;
using UnityEngine;

public class MobileUIBinder : MonoBehaviour {
    #region Variables
    [Header("Scene UI References")]
    [SerializeField] private VirtualJoystick moveJoystick;
    [SerializeField] private VirtualJoystick lookJoystick;

    private MobileInputBridge mobileInputBridge;
    #endregion

    #region Unity Methods
    private void OnEnable() {
        StartCoroutine(BindNextFrame());
    }
    #endregion

    #region Utility Methods
    private IEnumerator BindNextFrame() {
        yield return null;

        mobileInputBridge = FindFirstObjectByType<MobileInputBridge>();

        if (mobileInputBridge == null) {
            Debug.LogWarning("[MobileUIBinder] No MobileInputBridge found.");
            yield break;
        }

        if (moveJoystick == null) {
            Debug.LogWarning("[MobileUIBinder] moveJoystick is not assigned.");
        }

        if (lookJoystick == null) {
            Debug.LogWarning("[MobileUIBinder] lookJoystick is not assigned.");
        }

        mobileInputBridge.SetJoysticks(moveJoystick, lookJoystick);

        Debug.Log($"[MobileUIBinder] Bound joysticks to bridge on {mobileInputBridge.name}");
    }
    #endregion
}