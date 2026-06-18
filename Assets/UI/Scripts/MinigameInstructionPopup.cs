using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinigameInstructionPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button confirmButton;

    private Action onClosed;

    private void Awake()
    {
        confirmButton.onClick.AddListener(ClosePopup);

        if (root != null)
            root.SetActive(false);
    }

    public void Show(string title, string body, Action closedCallback = null)
    {
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;

        onClosed = closedCallback;

        if (root != null)
            root.SetActive(true);
    }

    public void ClosePopup()
    {
        if (root != null)
            root.SetActive(false);

        onClosed?.Invoke();
        onClosed = null;
    }

    public bool IsOpen()
    {
        return root != null && root.activeSelf;
    }
}