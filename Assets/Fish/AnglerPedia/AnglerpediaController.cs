using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnglerpediaController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private Anglerpedia database;
    [SerializeField] private PlayerDataRuntime playerData; // optional

    [Header("UI")]
    [SerializeField] private AnglerpediaDetailView detailView;
    [SerializeField] private AnglerpediaItemView[] itemViews; // drag 4 in inspector
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private TMP_Text pageInfoText; // "Page 1/3"

    [Header("Filter")]
    [SerializeField] private Environment currentEnvironmentFilter;

    private List<FishSpeciesConfig> _filteredFish = new List<FishSpeciesConfig>();
    private int _currentPageIndex;
    private const int ITEMS_PER_PAGE = 4;

    private void Awake()
    {
        nextPageButton.onClick.AddListener(NextPage);
        prevPageButton.onClick.AddListener(PrevPage);
    }

    private void OnEnable()
    {
        BuildFilteredList();
        _currentPageIndex = 0;
        RefreshPage();
        AutoSelectFirstOnPage();
    }

    private void BuildFilteredList()
    {
        _filteredFish.Clear();
        if (database == null) return;

        foreach (var fish in database.GetAll())
        {
            if (fish == null || !fish.environments.Contains(currentEnvironmentFilter))
                continue;

            _filteredFish.Add(fish);
        }
    }

    private void RefreshPage()
    {
        int startIndex = _currentPageIndex * ITEMS_PER_PAGE;

        for (int i = 0; i < itemViews.Length; i++)
        {
            int fishIndex = startIndex + i;
            if (fishIndex < _filteredFish.Count)
            {
                var fish = _filteredFish[fishIndex];
                bool discovered = playerData?.IsFishDiscovered(fish.id) ?? fish.isDiscovered; // fallback to SO flag

                itemViews[i].gameObject.SetActive(true);
                itemViews[i].Bind(fish, discovered, OnItemClicked);
            }
            else
            {
                itemViews[i].gameObject.SetActive(false);
            }
        }

        // Update pagination UI
        int maxPage = Mathf.Max(0, (_filteredFish.Count - 1) / ITEMS_PER_PAGE);
        prevPageButton.interactable = _currentPageIndex > 0;
        nextPageButton.interactable = _currentPageIndex < maxPage;
        pageInfoText.text = $"Page {_currentPageIndex + 1}/{maxPage + 1} ({_filteredFish.Count} species)";
    }

    private void AutoSelectFirstOnPage()
    {
        int startIndex = _currentPageIndex * ITEMS_PER_PAGE;
        if (_filteredFish.Count > startIndex)
        {
            detailView.Show(_filteredFish[startIndex]);
        }
        else
        {
            detailView.Show(null);
        }
    }

    private void OnItemClicked(FishSpeciesConfig fish)
    {
        detailView.Show(fish);
    }

    private void NextPage() { if (_currentPageIndex < Mathf.Max(0, (_filteredFish.Count - 1) / ITEMS_PER_PAGE)) { _currentPageIndex++; RefreshPage(); AutoSelectFirstOnPage(); } }
    private void PrevPage() { if (_currentPageIndex > 0) { _currentPageIndex--; RefreshPage(); AutoSelectFirstOnPage(); } }
}