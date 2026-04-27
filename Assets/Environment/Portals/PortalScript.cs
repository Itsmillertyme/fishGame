using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalScript : MonoBehaviour {
    #region Variables
    [SerializeField] private string portalDestinationSceneName;
    [SerializeField] private string displayName;
    [SerializeField] private string hubReturnSpawnId = "";
    #endregion

    #region Unity Methods
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            MobileInputBridge mib = FindAnyObjectByType<MobileInputBridge>();

            if (mib != null) {
                mib.ClearInputs();
            }

            if (!string.IsNullOrEmpty(hubReturnSpawnId)) {
                HubPlayerSpawner.activeSpawnId = hubReturnSpawnId;
            }

            SceneManager.LoadScene(portalDestinationSceneName);
        }
    }

    public void SetPortalActive(bool isActive) {

        GameObject portalEffect = transform.GetChild(0).gameObject;
        BoxCollider portalTriggerCollider = GetComponent<BoxCollider>();
        TextMeshProUGUI portalText = GetComponentInChildren<TextMeshProUGUI>();

        portalEffect.SetActive(isActive);
        portalTriggerCollider.isTrigger = isActive;

        portalText.text = isActive ? displayName : "Locked";
    }
    #endregion
}