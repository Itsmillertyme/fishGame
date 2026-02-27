using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DemoUIController : MonoBehaviour {

    #region Variables
    [SerializeField] Slider minigameToggleSlider;
    [SerializeField] TextMeshProUGUI minigameToggleLabel;
    [SerializeField] GameObject minigameControls;

    [SerializeField] TackleBox playerTackleBox;

    [SerializeField] ReelItem spinningReel;
    [SerializeField] ReelItem castingReel;


    bool showingControls = true;
    bool isInSpinningMode = true;

    #endregion

    #region Unity Methods

    #endregion

    #region Utility Methods
    public void ChangeMinigameMode() {

        if (!isInSpinningMode) {
            minigameToggleLabel.text = "Spinning Rod";
            playerTackleBox.EquipReel(spinningReel);
            minigameToggleSlider.value = 0;
            isInSpinningMode = true;
        }
        else {
            minigameToggleLabel.text = "Casting Rod";
            playerTackleBox.EquipReel(castingReel);
            minigameToggleSlider.value = 1;
            isInSpinningMode = false;
        }

    }

    public void ShowHideControls() {
        if (showingControls) {
            minigameControls.SetActive(false);
            showingControls = false;
        }
        else {
            minigameControls.SetActive(true);
            showingControls = true;
        }
    }
    #endregion

}
