using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnglerpediaItemView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image trophyBadge;

    private FishSpeciesConfig _fish;
    private bool _isDiscovered;
    private System.Action<FishSpeciesConfig> _onClicked;

    public void Bind(FishSpeciesConfig fish, bool isDiscovered, System.Action<FishSpeciesConfig> onClicked)
    {
        _fish = fish;
        _isDiscovered = isDiscovered;
        _onClicked = onClicked;

        if (fish != null)
        {
            iconImage.sprite = fish.icon;
            nameText.text = isDiscovered ? fish.displayName : "???";
            //trophyBadge.gameObject.SetActive(fish.isTrophy);
        }

        lockedOverlay.SetActive(!isDiscovered);
        button.interactable = isDiscovered;
    }

    private void Awake()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnClick() => _onClicked?.Invoke(_fish);
}