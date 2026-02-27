using UnityEngine;

public class PhoneMenuController : MonoBehaviour {
    public GameObject phoneMenu;
    public GameObject phoneWidget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        phoneWidget.SetActive(true);
        phoneMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnButtonClick() {
        if (!phoneMenu.activeInHierarchy) {
            phoneMenu.SetActive(true);
        }
        else if (phoneMenu.activeInHierarchy) {
            phoneMenu.SetActive(false);
        }
    }
}
