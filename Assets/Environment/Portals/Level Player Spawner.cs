using UnityEngine;

public class LevelPlayerSpawner : MonoBehaviour {
    #region Variables
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float yOffset = 0.1f;
    #endregion

    #region Unity Methods
    private void Start() {
        MovePlayerToSpawn();
    }
    #endregion

    #region Utility Methods
    private void MovePlayerToSpawn() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) {
            Debug.LogWarning("Player not found in scene.");
            return;
        }

        CharacterController controller = player.transform.GetChild(2).GetComponent<CharacterController>();

        if (controller != null) {
            controller.enabled = false;
        }

        controller.transform.position = spawnPoint.position + Vector3.up * yOffset;
        controller.transform.rotation = spawnPoint.rotation;

        if (controller != null) {
            controller.enabled = true;
        }
    }
    #endregion
}