using UnityEngine;
using UnityEngine.SceneManagement;

public class HubPlayerSpawner : MonoBehaviour {
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    //private void Start()
    //{
    //    if (GameObject.FindGameObjectWithTag("Player") == null)
    //    {
    //        Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    //    }
    //}
    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name != "Hub") return;

        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

        if (existingPlayer != null) return;

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position + 5 * Vector3.up, spawnPoint.rotation);

        //playerInstance.transform.GetChild(2).localScale *= 20;
    }

}