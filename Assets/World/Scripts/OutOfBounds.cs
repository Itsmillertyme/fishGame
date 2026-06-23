using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = spawnPoint.position;
            other.transform.rotation = spawnPoint.rotation;
        }
    }
}
