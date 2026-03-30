using UnityEngine;

public class AnglerTest : MonoBehaviour
{
    public GameObject anglerpedia;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (anglerpedia != null)
            {
                if (anglerpedia.activeInHierarchy)
                {
                    anglerpedia.SetActive(false);
                }
                else
                {
                    anglerpedia.SetActive(true);
                }
            }
        }
    }
}
