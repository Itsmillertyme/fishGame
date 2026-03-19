using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class PhoneMenuController : MonoBehaviour {
    public GameObject phoneMenu;
    public GameObject phoneWidget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        phoneWidget.SetActive(true);
        phoneMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OnButtonClick();
        }
#endif
    }

    public void OnButtonClick() {
        if (!phoneMenu.activeInHierarchy) {
            phoneMenu.SetActive(true);
            phoneWidget.SetActive(false);
        }
        else if (phoneMenu.activeInHierarchy)
        {
            phoneMenu.SetActive(false);
            phoneWidget.SetActive(true);
        }
    }
}
