using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnglerpediaItemView : MonoBehaviour {
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image trophyBadge;

    private FishSpeciesConfig _fish;
    private bool _isDiscovered;
    private System.Action<FishSpeciesConfig> _onClicked;

    public void Bind(FishSpeciesConfig fish, bool isDiscovered, System.Action<FishSpeciesConfig> onClicked) {
        _fish = fish;
        _isDiscovered = isDiscovered;
        _onClicked = onClicked;

        //JACOB CHANGED LINE 25, 29 & 30 TO USE fish.isDiscovered IN THE GAMEPLAY DEMO BECUASE IT WAS ALWAYS SHOWING FALSE, EVEN WHEN IT SHOULDN'T

        if (fish != null) {
            iconImage.sprite = fish.icon;
            nameText.text = fish.isDiscovered ? fish.displayName : "???";
            //trophyBadge.gameObject.SetActive(fish.isTrophy);
        }

        lockedOverlay.SetActive(!fish.isDiscovered);
        button.interactable = fish.isDiscovered;
    }

    private void Awake() {
        button.onClick.AddListener(OnClick);
    }

    private void OnClick() => _onClicked?.Invoke(_fish);
}