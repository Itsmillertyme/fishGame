using UnityEngine;
using UnityEngine.InputSystem;

public class PhoneMenuController : MonoBehaviour {
    public GameObject phoneMenu;
    public GameObject phoneWidget;

    public GameObject anglerpedia;
    public GameObject settingsMenu;
    public GameObject tacklebox;

    private void Start() {
        phoneWidget.SetActive(true);
        phoneMenu.SetActive(false);

        anglerpedia.SetActive(false);
        settingsMenu.SetActive(false);
    }

    private void Update() {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
            OpenAnglerpedia();

        if (Keyboard.current.eKey.wasPressedThisFrame)
            OpenSettings();

        if (Keyboard.current.rKey.wasPressedThisFrame)
            ClosePhoneMenu();
#endif
    }

    public void OnPhoneButtonClick() {
        if (!phoneMenu.activeSelf) {
            phoneMenu.SetActive(true);
            phoneWidget.SetActive(false);

            //Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;
        }
        else {
            //Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;
            ClosePhoneMenu();

        }
    }

    public void OpenAnglerpedia() {
        phoneMenu.SetActive(true);
        phoneWidget.SetActive(false);

        anglerpedia.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void OpenSettings() {
        phoneMenu.SetActive(true);
        phoneWidget.SetActive(false);

        settingsMenu.SetActive(true);
        anglerpedia.SetActive(false);
    }

    public void ClosePhoneMenu() {
        phoneMenu.SetActive(false);
        phoneWidget.SetActive(true);

        anglerpedia.SetActive(false);
        settingsMenu.SetActive(false);
        tacklebox.SetActive(false);
    }
}
