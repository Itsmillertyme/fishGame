using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class EndGame : MonoBehaviour {

    public void QuitGame() {
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
            // Quit the standalone application build
            Application.Quit();
#endif
    }
    
}
