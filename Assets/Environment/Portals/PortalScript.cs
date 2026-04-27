using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalScript : MonoBehaviour {
    #region Variables
    [SerializeField] private string portal;
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

            SceneManager.LoadScene(portal);
        }
    }
    #endregion
}