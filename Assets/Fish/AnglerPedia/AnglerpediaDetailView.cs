using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnglerpediaDetailView : MonoBehaviour
{
    [Header("Basic Info")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text environmentsText;
    [SerializeField] private Image trophyBadge;

    [Header("Stats")]
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Image strengthBar, aggressionBar, curiosityBar; // or sliders

    [Header("Size Info")]
    [SerializeField] private TMP_Text sizeText;

    [Header("Pictures")]
    [SerializeField] private Image pictureImage; // cycle through caughtPictures
    [SerializeField] private Button nextPictureButton;

    private FishSpeciesConfig _currentFish;
    private int _currentPictureIndex;

    public void Show(FishSpeciesConfig fish)
    {
        if (fish == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        _currentFish = fish;
        _currentPictureIndex = 0;

        // Basic info
        iconImage.sprite = fish.icon;
        nameText.text = fish.displayName;
        descriptionText.text = fish.description;
        environmentsText.text = string.Join(", ", fish.environments);
        //trophyBadge.gameObject.SetActive(fish.isTrophy);

        // Stats
        statsText.text = $"Strength: {fish.strength}/5\nAggression: {fish.aggression}/5\nCuriosity: {fish.curiosity}/5";

        // Size range
        fish.TryResolveLengthWeight(0f, out float minLen, out float minWt);
        fish.TryResolveLengthWeight(1f, out float maxLen, out float maxWt);
        sizeText.text = $"Size: {minLen:F1}-{maxLen:F1}\" ({minWt:F1}-{maxWt:F1} lb)";

        // Picture
        ShowCurrentPicture();
        nextPictureButton.interactable = fish.caughtPictures.Count > 1;
    }

    private void ShowCurrentPicture()
    {
        if (_currentFish?.caughtPictures != null && _currentPictureIndex < _currentFish.caughtPictures.Count)
        {
            pictureImage.sprite = _currentFish.caughtPictures[_currentPictureIndex];
        }
    }

    public void NextPicture()
    {
        if (_currentFish == null) return;

        _currentPictureIndex = (_currentPictureIndex + 1) % _currentFish.caughtPictures.Count;
        ShowCurrentPicture();
    }
}