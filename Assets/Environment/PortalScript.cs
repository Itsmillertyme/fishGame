using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalScript : MonoBehaviour
{
    [SerializeField] private string portal;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MobileInputBridge mib = FindAnyObjectByType<MobileInputBridge>();
            mib.ClearInputs();
            SceneManager.LoadScene(portal);
        }
    }
}