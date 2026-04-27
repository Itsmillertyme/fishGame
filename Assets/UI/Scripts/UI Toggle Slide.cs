using UnityEngine;

public class UIToggleSlide : MonoBehaviour {
    [Header("References")]
    [SerializeField] private RectTransform panel;

    [Header("Positions")]
    [SerializeField] private Vector2 shownPosition;
    [SerializeField] private Vector2 hiddenPosition;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 10f;

    private bool isShown = false;
    private Vector2 targetPosition;

    private void Start() {
        if (panel == null) {
            panel = GetComponent<RectTransform>();
        }

        // Start hidden
        panel.anchoredPosition = hiddenPosition;
        targetPosition = hiddenPosition;
    }

    private void Update() {
        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            targetPosition,
            Time.deltaTime * moveSpeed
        );
    }

    public void Toggle() {
        isShown = !isShown;
        targetPosition = isShown ? shownPosition : hiddenPosition;
    }
}