using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
    #region Variables

    [Header("References")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField] private Canvas canvas;

    [Header("Settings")]
    [SerializeField] private float handleRange = 60f;
    [SerializeField] private float deadZone = 0.1f;
    [SerializeField] private bool normalizeOutput = true;

    public Vector2 Value { get; private set; }

    private Camera uiCamera;
    #endregion

    #region Unity Methods
    private void Awake() {
        if (canvas == null) {
            canvas = GetComponentInParent<Canvas>();
        }

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) {
            uiCamera = canvas.worldCamera;
        }

        ResetJoystick();
    }

    public void OnPointerDown(PointerEventData eventData) {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData) {
        if (background == null || handle == null)
            return;

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, uiCamera, out localPoint)) {
            return;
        }

        Vector2 raw = localPoint / handleRange;
        raw = Vector2.ClampMagnitude(raw, 1f);

        float magnitude = raw.magnitude;

        if (magnitude < deadZone) {
            Value = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
            return;
        }

        if (normalizeOutput) {
            Value = raw.normalized * Mathf.InverseLerp(deadZone, 1f, magnitude);
        }
        else {
            Value = raw;
        }

        handle.anchoredPosition = Value * handleRange;
    }

    public void OnPointerUp(PointerEventData eventData) {
        ResetJoystick();
    }

    #endregion

    #region Utility Methods
    public void ResetJoystick() {
        Value = Vector2.zero;

        if (handle != null) {
            handle.anchoredPosition = Vector2.zero;
        }
    }
    #endregion
}