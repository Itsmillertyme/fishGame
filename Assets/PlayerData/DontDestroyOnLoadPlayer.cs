using UnityEngine;

public class DontDestroyOnLoadPlayer: MonoBehaviour
{
    private static DontDestroyOnLoadPlayer instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}