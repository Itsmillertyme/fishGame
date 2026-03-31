using UnityEngine;

public class BillboardWorldCanvas : MonoBehaviour {

    void LateUpdate() {
        transform.forward = Camera.main.transform.forward;
    }
}