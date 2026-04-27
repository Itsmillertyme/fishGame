using UnityEngine;
using UnityEngine.SceneManagement;

public class HubPlayerSpawner : MonoBehaviour {
    #region Variables
    public static string activeSpawnId = "Default";

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string defaultSpawnId = "Default";
    [SerializeField] private float yOffset = 0.1f;

    [SerializeField] AudioClip portalSFX;
    #endregion

    #region Unity Methods
    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    #region Utility Methods
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name != "Hub") return;

        string spawnId = string.IsNullOrEmpty(activeSpawnId) ? defaultSpawnId : activeSpawnId;
        Transform spawnPoint = FindSpawnPoint(spawnId);

        if (spawnPoint == null) {
            Debug.LogWarning("No hub spawn point found for ID: " + spawnId);
            return;
        }

        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

        if (existingPlayer == null) {
            Instantiate(playerPrefab, spawnPoint.position + Vector3.up * yOffset, spawnPoint.rotation);
        }
        else {
            MovePlayer(existingPlayer.transform.GetChild(2).gameObject, spawnPoint);
        }
    }

    private Transform FindSpawnPoint(string spawnId) {
        PlayerSpawnPoint[] spawnPoints = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);

        foreach (PlayerSpawnPoint spawnPoint in spawnPoints) {
            if (spawnPoint.spawnId == spawnId) {
                return spawnPoint.transform;
            }
        }

        return null;
    }

    private void MovePlayer(GameObject player, Transform spawnPoint) {
        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null) {
            controller.enabled = false;
        }

        controller.transform.localPosition = new Vector3(0, controller.transform.localPosition.y, 0);


        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;

        CameraController cameraController = controller.GetComponent<CameraController>();
        cameraController.SnapBehindPlayer(controller.transform);

        AudioSource.PlayClipAtPoint(
                    portalSFX,
                    transform.TransformPoint(controller.center),
                    0.5f
                );

        if (controller != null) {
            controller.enabled = true;
        }
    }
    #endregion
}